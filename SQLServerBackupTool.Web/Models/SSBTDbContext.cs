using System.ComponentModel;
using System.Data.Entity;

namespace SQLServerBackupTool.Web.Models
{
    [Localizable(false)]
    public class SSBTDbContext : DbContext
    {
        public SSBTDbContext()
            : base("name=DefaultConnection")
        {
        }

        public DbSet<UserDatabase> UserDatabases { get; set; }
        public DbSet<BackupHistory> History { get; set; }
    }
}
