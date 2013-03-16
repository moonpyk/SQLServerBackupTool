using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace SQLServerBackupTool
{
    internal class BackupUtil : IDisposable
    {
        public const string BackupCommandTemplate = @"
BACKUP DATABASE [{0}] 
TO  DISK = N'{1}' 
WITH NOFORMAT, NOINIT,  NAME = N'{0} - {2}', SKIP, NOREWIND, NOUNLOAD,  STATS = 10;
";

        private readonly SqlConnection _co;
        private bool _disposed;

        public BackupUtil(string connectionString)
        {
            _co = new SqlConnection(connectionString);
        }

        public bool IsConnectionOpened
        {
            get;
            protected set;
        }

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

        public void Close()
        {
            if (!IsConnectionOpened)
            {
                return;
            }

            _co.Close();
        }

        public int BackupDatabase(string databaseName, string backupPath, DateTime ts)
        {
            if (!IsConnectionOpened)
            {
                Open();
            }

            var q = _co.CreateCommand();

            q.CommandText = string.Format(
                BackupCommandTemplate,
                databaseName,
                backupPath,
                string.Format("{0} {1}", ts.ToShortDateString(), ts.ToShortTimeString()
                    ));

            return q.ExecuteNonQuery();
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