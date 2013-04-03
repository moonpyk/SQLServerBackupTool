using SQLServerBackupTool.Lib.Annotations;

namespace SQLServerBackupTool.Web.Models
{
    [UsedImplicitly]
    public class SchemaInfo
    {
        public const string Query = @"
SELECT  sys.tables.name AS [Table] ,
        sys.columns.name AS [Column] ,
        sys.types.name AS [Type] ,
        sys.columns.max_length AS [Length] ,
        ( SELECT    CASE WHEN ( sys.columns.max_length <> -1 )
                         THEN CAST(sys.columns.max_length AS NVARCHAR(MAX))
                         ELSE 'MAX'
                    END
        ) AS [LengthString]
FROM    sys.tables
        INNER JOIN sys.columns ON sys.tables.object_id = sys.columns.object_id
        INNER JOIN sys.types ON sys.columns.system_type_id = sys.types.system_type_id
WHERE   sys.types.NAME NOT IN ( 'sysname' )
        AND sys.tables.NAME NOT IN ( 'sysdiagrams' );
";

        public const string RowCountQuery = @"
SELECT COUNT(*) FROM {0};
";

        public string Table
        {
            get;
            set;
        }

        public string Column
        {
            get;
            set;
        }

        public string Type
        {
            get;
            set;
        }

        public int Length
        {
            get;
            set;
        }

        public string LengthString
        {
            get;
            set;
        }

        public int RowCount
        {
            get;
            set;
        }
    }
}