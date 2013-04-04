using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SQLServerBackupTool.Lib
{
    public class SqlServerBackupProvider : ISqlBackupProvider
    {
        public const string BackupCommandTemplate = @"
BACKUP DATABASE [{0}] 
TO  DISK = N'{1}' 
WITH NOFORMAT, NOINIT, NAME = N'{0} - {2}', SKIP, NOREWIND, NOUNLOAD, STATS = 10;
";

        private readonly DbConnection _co;
        private bool _disposed;

        public SqlServerBackupProvider(string connectionString)
        {
            _co = new SqlConnection(connectionString);
        }

        /// <summary>
        /// Indicates if the underlaying SQL Connection is opened
        /// </summary>
        public bool IsConnectionOpened
        {
            get;
            protected set;
        }

        /// <summary>
        /// Opens the underlaying SQL Connection
        /// </summary>
        public void Open()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(
                    "Unable to open a data connection from a disposed BackupUtil",
                    innerException: null
                );
            }

            _co.Open();
            IsConnectionOpened = true;
        }

        /// <summary>
        /// Async proxy to <see cref="Open"/>
        /// </summary>
        public Task OpenAsync()
        {
            return Task.Factory.StartNew(Open);
        }

        /// <summary>
        /// Closes the underlaying SQL Connecion
        /// </summary>
        public void Close()
        {
            if (!IsConnectionOpened)
            {
                return;
            }

            _co.Close();
        }

        /// <summary>
        /// Instructs SQL Server to make a backup of the given database, path with specified timestamp
        /// </summary>
        /// <param name="databaseName">Name of the database</param>
        /// <param name="backupPath">Full destination file name of the database backup</param>
        /// <param name="ts">Timestamp, used for indicative purposes inside the resulting backup metadata</param>
        /// <returns>Return value of <see cref="SqlCommand.ExecuteNonQuery"/></returns>
        public int BackupDatabase(string databaseName, string backupPath, DateTime ts)
        {
            if (!IsConnectionOpened)
            {
                Open();
            }

            var q = _co.CreateCommand();

            q.CommandTimeout = 0; // Backups can take a long time for big databases
            q.CommandText    = string.Format(
                BackupCommandTemplate,
                databaseName,
                backupPath,
                string.Format("{0} {1}", ts.ToShortDateString(), ts.ToShortTimeString()
            ));

            return q.ExecuteNonQuery();
        }

        /// <summary>
        /// Async proxy to <see cref="BackupDatabase"/>
        /// </summary>
        public Task<int> BackupDatabaseAsync(string databaseName, string backupPath, DateTime ts)
        {
            return Task.Factory.StartNew(() => BackupDatabase(databaseName, backupPath, ts));
        }

        /// <summary>
        /// Exécute les tâches définies par l'application associées à la libération ou à la redéfinition des ressources non managées.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Close();

            _co.Dispose();
            _disposed = true;
        }
    }
}