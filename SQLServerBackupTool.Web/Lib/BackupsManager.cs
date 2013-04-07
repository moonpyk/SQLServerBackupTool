using Dapper;
using Ionic.Zip;
using NLog;
using SQLServerBackupTool.Lib;
using SQLServerBackupTool.Lib.Annotations;
using SQLServerBackupTool.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;

namespace SQLServerBackupTool.Web.Lib
{
    public static class BackupsManager
    {
        public static readonly XmlSerializer XmlSerializer = new XmlSerializer(typeof(BackupHistory));

        /// <summary>
        /// Fetches the list of databases available on SQLServer, with additional information.
        /// </summary>
        /// <param name="user"><see cref="IPrincipal"/> used to filter final results with authorized databases</param>
        /// <returns>A list of <see cref="DatabaseInfo"/></returns>
        public static async Task<IList<DatabaseInfo>> GetDatabasesInfo(IPrincipal user)
        {
            using (var co = new SqlConnection(GetBackupsConnectionString()))
            {
                await co.OpenAsync();
                var p = await Task.Run(() => co.Query<DatabaseInfo>(DatabaseInfo.Query).OrderBy(_ => _.Id).ToList());

                foreach (var info in p.Where(_ => _.IsOnline))
                {
                    co.ChangeDatabase(info.Name);
                    var size = await Task.Run(() => co.Query<DatabaseSizeInfo>(DatabaseSizeInfo.Query).First());

                    if (size != null && !string.IsNullOrEmpty(size.DatabaseSize))
                    {
                        info.Size = size.DatabaseSize;
                    }
                }
                return p;
            }
        }

        /// <summary>
        /// Backup implementation logic, asks SQLServer to make a backup, of <see cref="dbName"/>, creates a Zip archive on success, tries to delete the original backup file.
        /// </summary>
        /// <param name="backupConnectionString">Connection string to use to instruct SQLServer to make the backup</param>
        /// <param name="dbName">Name of the database to backup</param>
        /// <param name="logger">Optional instance of a <see cref="Logger"/></param>
        /// <returns>An instance of <see cref="BackupHistory"/> on success, null otherwise</returns>
        /// <remarks>If deletion of the original backup file fails for any reason, only a log entry will be done, the backup won't be considered as failed.
        /// This methods needs to be run in a valid <see cref="HttpContext"/>.
        /// </remarks>
        public static async Task<BackupHistory> BackupDatabase([NotNull] string backupConnectionString, [NotNull] string dbName, [CanBeNull] Logger logger)
        {
            if (string.IsNullOrWhiteSpace(backupConnectionString))
            {
                throw new ArgumentNullException("backupConnectionString");
            }

            if (string.IsNullOrWhiteSpace(dbName))
            {
                throw new ArgumentNullException("dbName");
            }

            var httpContext = HttpContext.Current;

            if (httpContext == null)
            {
                throw new Exception("HttpContext.Current returned null");
            }

            var server = httpContext.Server;
            var user   = httpContext.User;

            using (var bak = new SqlServerBackupProvider(backupConnectionString))
            {
                await bak.OpenAsync();
                var ts = DateTime.Now;

                var backupsPath = server.MapPath("~/Backups");

                var fNameBase = Utils.GenerateBackupBaseName(dbName, ts);

                var fullBackupPath = Path.Combine(backupsPath, string.Format("{0}.bak", fNameBase));

                try
                {
                    await bak.BackupDatabaseAsync(dbName, fullBackupPath, ts);
                }
                catch (Exception ex)
                {
                    if (logger != null)
                    {
                        logger.ErrorException("During database backup", ex);
                    }
                    return null;
                }

                var fullZipPath = Path.Combine(backupsPath, string.Format("{0}.zip", fNameBase));

                try
                {
                    using (var z = new ZipFile(fullZipPath))
                    {
                        z.AddFile(fullBackupPath, string.Empty);

                        await Task.Run(() => z.Save());
                    }
                }
                catch (Exception ex)
                {
                    if (logger != null)
                    {
                        logger.ErrorException("During zip file creation", ex);
                    }
                    return null;
                }

                try
                {
                    File.Delete(fullBackupPath);
                }
                catch (Exception ex)
                {
                    if (logger != null)
                    {
                        logger.ErrorException("During original file deletion", ex);
                    }
                }

                var h = new BackupHistory
                {
                    Path     = fullZipPath,
                    Database = dbName,
                    Url      = string.Format("~/{0}", fullZipPath.Replace(server.MapPath("~/"), string.Empty).Replace('\\', '/')),
                    Expires  = DateTime.Now.AddDays(1),
                    Username = user.Identity.Name,
                };

                using (var ddb = new SSBTDbContext())
                {
                    ddb.History.Add(h);
                    ddb.SaveChanges();
                }

                return h;
            }
        }

        /// <summary>
        /// Backup purging deletion logic. Each backup is created with an <see cref="BackupHistory.Expires"/> DateTime, all backups 
        /// that reached the expiration date will be deleted, their <see cref="SSBTDbContext.History"/> entry deleted too.
        /// </summary>
        /// <param name="ddb">An instance of <see cref="SSBTDbContext"/> </param>
        /// <param name="from">Reference date to compare <see cref="BackupHistory.Expires"/> with</param>
        /// <param name="logger">Optional instance of a <see cref="Logger"/></param>
        /// <returns>true if nothing was expired, or if everything went good during purge. false if <see cref="DbContext.SaveChanges"/> encountered an exception.</returns>
        public static bool PurgeOldBackups([NotNull] SSBTDbContext ddb, DateTime from, [CanBeNull] Logger logger)
        {
            if (ddb == null)
            {
                throw new ArgumentNullException("ddb");
            }

            var oldBackups = ddb.History
                .Where(_ => from > _.Expires)
                .ToList();

            var didChange = oldBackups.Any(_ => DeleteBackup(ddb, _, logger));

            if (!didChange)
            {
                return true;
            }

            try
            {
                ddb.SaveChanges();
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    logger.ErrorException("While saving database changes", ex);
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// Backup deletion logic, deletes a database backup represented as a <see cref="BackupHistory"/>.
        /// Deletes the recorded archive file then marks the <see cref="BackupHistory"/> entity for deletion in <see cref="ddb"/>
        /// </summary>
        /// <param name="ddb">An instance of <see cref="SSBTDbContext"/></param>
        /// <param name="b">The <see cref="BackupHistory"/> instance to delete</param>
        /// <param name="logger">Optional instance of a <see cref="Logger"/></param>
        /// <returns>true if everything went good, false otherwise.</returns>
        /// <remarks>If the database backup archive fails to be deleted for any reason, a log will be written but the <see cref="b"/> won't be marked for deletion.</remarks>
        public static bool DeleteBackup([NotNull] SSBTDbContext ddb, BackupHistory b, [CanBeNull] Logger logger)
        {
            if (ddb == null)
            {
                throw new ArgumentNullException("ddb");
            }

            try
            {
                File.Delete(b.Path);
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    logger.ErrorException(String.Format("During backup file deletion '{0}'", b.Path), ex);
                }
                return false;
            }

            ddb.History.Remove(b);
            return true;
        }

        public static string GetBackupsConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["BackupConnection"].ConnectionString;
        }
    }
}
