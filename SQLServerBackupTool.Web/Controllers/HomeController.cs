using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Mvc;
using Dapper;
using SQLServerBackupTool.Web.Models;

namespace SQLServerBackupTool.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        public ActionResult Index()
        {
            using (var co = new SqlConnection(GetConnectionString()))
            {
                co.Open();
                var p = co.Query<DatabaseInfo>(DatabaseInfo.GetDatabasesNames);

                return View(p);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Index(string id)
        {
            // using (var bak = new SqlServerBackupProvider(GetConnectionString()))
            // {
            //     await bak.OpenAsync();
            // 
            // }

            return new EmptyResult();
        }

        private static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["BackupConnection"].ConnectionString;
        }
    }
}
