using System.Data.Entity;

namespace SQLServerBackupTool.Web.Models
{
    public class SSBTDbContext : DbContext
    {
        // Vous pouvez ajouter du code personnalisé à ce fichier. Les modifications ne seront pas remplacées.
        // 
        // Si vous voulez qu’Entity Framework abandonne et régénère la base de données
        // automatiquement à chaque fois que vous modifiez le schéma du modèle, ajoutez le code
        // suivant à la méthode Application_Start dans le fichier Global.asax.
        // Remarque : cette opération supprime et recrée la base de données à chaque modification du modèle.
        // 
        // System.Data.Entity.Database.SetInitializer(new System.Data.Entity.DropCreateDatabaseIfModelChanges<SQLServerBackupTool.Web.Models.SSBTDbContext>());

        public SSBTDbContext()
            : base("name=DefaultConnection")
        {
        }

        public DbSet<UserDatabase> UserDatabases { get; set; }
    }
}
