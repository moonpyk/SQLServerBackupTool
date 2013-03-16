using System;
using System.Data.SqlClient;

namespace SQLServerBackupTool.Lib
{
    internal interface ISqlBackupProvider : IDisposable
    {
        /// <summary>
        /// Instructs SQL Server to make a backup of the given database, path with specified timestamp
        /// </summary>
        /// <param name="databaseName">Name of the database</param>
        /// <param name="backupPath">Full destination file name of the database backup</param>
        /// <param name="ts">Timestamp, used for indicative purposes inside the resulting backup metadata</param>
        /// <returns>Indicative return code</returns>
        int BackupDatabase(string databaseName, string backupPath, DateTime ts);
    }
}