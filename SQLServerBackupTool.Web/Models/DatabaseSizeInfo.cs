using Dapper;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SQLServerBackupTool.Web.Models
{
    public class DatabaseSizeInfo
    {
        public const string Query = @"
EXEC sys.sp_spaceused;
";
        private static bool _typeBound;

        [Column("database_name")]
        public string DatabaseName
        {
            get;
            set;
        }

        [Column("database_size")]
        public string DatabaseSize
        {
            get;
            set;
        }

        [Column("unallocated space")]
        public string UnallocatedSpace
        {
            get;
            set;
        }

        public static void BindType()
        {
            if (_typeBound)
            {
                return;
            }

            _typeBound = true;

            // ReSharper disable ConvertToLambdaExpression : Unmaintainable code
            SqlMapper.SetTypeMap(typeof(DatabaseSizeInfo), new CustomPropertyTypeMap(typeof(DatabaseSizeInfo), delegate(Type type, string s)
            {
                return type.GetProperties().FirstOrDefault(_ => _.GetCustomAttributes(false).OfType<ColumnAttribute>().Any(attr => attr.Name == s));
            }));
            // ReSharper restore ConvertToLambdaExpression
        }
    }
}