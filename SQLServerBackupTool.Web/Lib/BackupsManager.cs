using NLog;
using SQLServerBackupTool.Lib.Annotations;
using SQLServerBackupTool.Web.Models;
using System;
using System.IO;
using System.Linq;

namespace SQLServerBackupTool.Web.Lib
{
    public static class BackupsManager
    {
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