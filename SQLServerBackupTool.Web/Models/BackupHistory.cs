using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SQLServerBackupTool.Web.Models
{
    [Table("BackupHistory")]
    public class BackupHistory
    {
        public int Id
        {
            get;
            set;
        }

        [MaxLength(255)]
        public string Url
        {
            get;
            set;
        }

        [MaxLength(255)]
        public string Path
        {
            get;
            set;
        }

        [MaxLength(255)]
        public string Username
        {
            get;
            set;
        }

        [MaxLength(50)]
        public string Database
        {
            get;
            set;
        }

        public DateTime Expires
        {
            get;
            set;
        }

        public long Size
        {
            get;
            set;
        }
    }
}
