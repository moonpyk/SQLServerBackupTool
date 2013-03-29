using Dapper;
using Ionic.Zip;
using SQLServerBackupTool.Lib;
using SQLServerBackupTool.Web.Lib.Mvc;
using SQLServerBackupTool.Web.Models;
using SQLServerBackupTool.Web.ViewModels;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
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
            using (var co = new SqlConnection(GetBackupsConnectionString()))
            {
                co.Open();
                var p = co.Query<DatabaseInfo>(DatabaseInfo.Query);

                IQueryable<BackupHistory> q = DbContext.History;

                if (!User.IsInRole("Admin"))
                {
                    q = q.Where(_ => _.Username == User.Identity.Name);
                }

                return View(new IndexViewModel(p, q));
            }
        }

        public ActionResult Schema(string id)
        {
            using (var co = new SqlConnection(GetBackupsConnectionString()))
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

        //[HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Backup(string id)
        {
            using (var bak = new SqlServerBackupProvider(GetBackupsConnectionString()))
            {
                await bak.OpenAsync();
                var ts = DateTime.Now;

                var backupsPath = Server.MapPath("~/Backups");

                var fNameBase = string.Format(
                    "{0}.{1}",
                    id,
                    ts.ToString("yyyyMMdd.HHmmss")
                );

                var backupName = string.Format("{0}.bak", fNameBase);

                var fullBackupPath = Path.Combine(backupsPath, backupName);

                try
                {
                    await bak.BackupDatabaseAsync(id, fullBackupPath, ts);
                }
                catch (Exception ex)
                {
                    Logger.ErrorException("During database backup", ex);
                    return HttpNotFound(string.Format("Unable to backup database '{0}'", id));
                }

                var fullZipPath = Path.Combine(backupsPath, string.Format("{0}.zip", fNameBase));

                try
                {
                    using (var z = new ZipFile(fullZipPath))
                    {
                        z.AddFile(fullBackupPath, string.Empty);

                        await Task.Run(() => z.Save());
                    }
                }
                catch (Exception ex)
                {
                    Logger.ErrorException("Furing zip file creation", ex);
                    return HttpNotFound(string.Format("Error during backup zip creation"));
                }

                try
                {
                    System.IO.File.Delete(fullBackupPath);
                }
                catch (Exception ex)
                {
                    Logger.ErrorException("During original file deletion", ex);
                }

                var h = new BackupHistory
                {
                    Path     = fullZipPath,
                    Url      = string.Format("~/{0}", fullZipPath.Replace(Server.MapPath("~/"), string.Empty).Replace('\\', '/')),
                    Expires  = DateTime.Now.AddDays(1),
                    Username = User.Identity.Name,
                };

                using (var ddb = new SSBTDbContext())
                {
                    ddb.History.Add(h);
                    ddb.SaveChanges();
                }

                return Json(h, JsonRequestBehavior.AllowGet);
            }
        }

        // [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
        public ActionResult PurgeOldBackups()
        {
            if (PurgeOldBackupsImpl(DateTime.Now))
            {
                this.AddFlashMessage("Outdated backups successfully removed", FlashMessageType.Success);
            }
            else
            {
                this.AddFlashMessage("An error occured during purging", FlashMessageType.Error);
            }

            return RedirectToAction("Index");
        }

        //[HttpPost, ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var bk = DbContext.History.Find(id);

            if (bk == null)
            {
                return HttpNotFound("Not a valid backup id");
            }

            if (DeleteBackup(bk))
            {
                try
                {
                    DbContext.SaveChanges();
                    return Content("OK", "text/plain");

                }
                catch (Exception ex)
                {
                    Logger.ErrorException("While saving database changes", ex);
                }
            }

            return Content("ERR", "text/plain");
        }

        private bool PurgeOldBackupsImpl(DateTime from)
        {
            var oldBackups = DbContext.History
                .Where(_ => from > _.Expires)
                .ToList();

            var didChange = false;

            foreach (var b in oldBackups.Where(DeleteBackup))
            {
                didChange = true;
            }

            if (!didChange)
            {
                return true;
            }

            try
            {
                DbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.ErrorException("While saving database changes", ex);
                return false;
            }

            return true;
        }

        private bool DeleteBackup(BackupHistory b)
        {
            try
            {
                System.IO.File.Delete(b.Path);
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("During backup file deletion '{0}'", b.Path), ex);
                return false;
            }

            DbContext.History.Remove(b);
            return true;
        }

        private static string GetBackupsConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["BackupConnection"].ConnectionString;
        }
    }
}
