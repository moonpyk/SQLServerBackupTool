using SQLServerBackupTool.Lib.Annotations;
using System.ComponentModel.DataAnnotations;

namespace SQLServerBackupTool.Web.Models
{
    [UsedImplicitly]
    public class UserDatabase
    {
        [Key]
        public int Id
        {
            get;
            set;
        }

        [MaxLength(50), Required]
        public string Username
        {
            get;
            set;
        }

        [MaxLength(50), Required]
        public string DatabaseName
        {
            get;
            set;
        }
    }
}
