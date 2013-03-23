using Dapper;
using SQLServerBackupTool.Web.Lib.Mvc;
using SQLServerBackupTool.Web.Models;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SQLServerBackupTool.Web.Controllers
{
    public class HomeController : ApplicationController
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
