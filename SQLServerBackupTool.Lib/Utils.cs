using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLServerBackupTool.Lib
{
    public static class Utils
    {
        /// <summary>
        /// Utility function to generate an meaningful backup base name with a timestamp
        /// </summary>
        /// <param name="dbName">Name of the database</param>
        /// <param name="ts">Timestamp</param>
        /// <returns>Something like dbName.yyyyMMdd.HHmmss</returns>
        public static string GenerateBackupBaseName(string dbName, DateTime ts)
        {
            return string.Format(
                "{0}.{1}",
                dbName,
                ts.ToString("yyyyMMdd.HHmmss")
            );
        }
    }
}
