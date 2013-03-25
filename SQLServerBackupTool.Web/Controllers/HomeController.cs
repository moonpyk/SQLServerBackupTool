using System.Collections.Generic;
using Dapper;
using SQLServerBackupTool.Web.Lib.Mvc;
using SQLServerBackupTool.Web.Models;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
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
                var p = co.Query<DatabaseInfo>(DatabaseInfo.Query);

                return View(p);
            }
        }

        public ActionResult Schema(string id)
        {
            using (var co = new SqlConnection(GetConnectionString()))
            {
                co.Open();

                try
                {
                    co.ChangeDatabase(id);
                }
                catch (SqlException) // Schema doesn't exists, usually
                {
                    return RedirectToAction("Index");
                }

                var schem = co.Query<SchemaInfo>(SchemaInfo.Query).ToList();
                var tList = schem.Select(_ => _.Table).Distinct();

                foreach (var t in tList)
                {
                    var rc = co.Query<int>(string.Format(SchemaInfo.RowCountQuery, t)).First();

                    var __ = t;
                    foreach (var c in schem.Where(_ => _.Table == __))
                    {
                        c.RowCount = rc;
                    }
                }

                ViewBag.Database = id;
                return View(schem);
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
