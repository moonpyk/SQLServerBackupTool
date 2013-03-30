using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLServerBackupTool.Lib
{
    public static class Utils
    {
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
