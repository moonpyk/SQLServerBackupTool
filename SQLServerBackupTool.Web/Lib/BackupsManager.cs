using Ionic.Zip;
using NLog;
using SQLServerBackupTool.Lib;
using SQLServerBackupTool.Lib.Annotations;
using SQLServerBackupTool.Web.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SQLServerBackupTool.Web.Lib
{
    public static class BackupsManager
    {
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
                        logger.ErrorException("Furing zip file creation", ex);
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
    }
}