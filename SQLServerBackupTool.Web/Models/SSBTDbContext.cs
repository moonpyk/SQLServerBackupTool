using System.Data.Entity;

namespace SQLServerBackupTool.Web.Models
{
    public class SSBTDbContext : DbContext
    {       
        public SSBTDbContext()
            : base("name=DefaultConnection")
        {
        }

        public DbSet<UserDatabase> UserDatabases { get; set; }
    }
}
