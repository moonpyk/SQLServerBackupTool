using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SQLServerBackupTool.Web.Models
{
    public class DatabaseInfo
    {
        public const string Query = @"
SELECT  database_id AS Id,
        name as Name
FROM    sys.databases
WHERE   name NOT IN ( 'master', 'tempdb', 'model', 'msdb' );
";

        public int Id { get; set; }
        public string Name { get; set; }
    }
}