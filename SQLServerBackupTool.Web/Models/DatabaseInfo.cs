using SQLServerBackupTool.Lib.Annotations;

namespace SQLServerBackupTool.Web.Models
{
    [UsedImplicitly]
    public class DatabaseInfo
    {
        public const string Query = @"
SELECT  database_id AS Id ,
        name AS Name ,
        ( CASE WHEN state = 0 THEN 1
               ELSE 0
          END ) AS Online
FROM    sys.databases
WHERE   name NOT IN ( 'master', 'tempdb', 'model', 'msdb', 'sysdb' );
";

        public int Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public bool Online
        {
            get;
            set;
        }
    }
}