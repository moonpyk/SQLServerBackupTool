using Dapper;
using SQLServerBackupTool.Web.Lib;
using SQLServerBackupTool.Web.Lib.Mvc;
using SQLServerBackupTool.Web.Models;
using SQLServerBackupTool.Web.ViewModels;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml.Serialization;

namespace SQLServerBackupTool.Web.Controllers
{
    public class HomeController : ApplicationController
    {
        private static readonly XmlSerializer BackupHistoryXmlSerializer = new XmlSerializer(typeof(BackupHistory));

        //
        // GET: /Home/
        public async Task<ActionResult> Index()
        {
            IQueryable<BackupHistory> q = DbContext.History;

            if (!User.IsInRole("Admin"))
            {
                q = q.Where(_ => _.Username == User.Identity.Name);
            }

            q = q.OrderBy(_ => _.Id);

            using (var co = new SqlConnection(GetBackupsConnectionString()))
            {
                await co.OpenAsync();
                var p = await Task.Run(() => co.Query<DatabaseInfo>(DatabaseInfo.Query).OrderBy(_ => _.Id));

                return View(new IndexViewModel(p, q));
            }
        }

        public async Task<ActionResult> Schema(string id)
        {
            using (var co = new SqlConnection(GetBackupsConnectionString()))
            {
                await co.OpenAsync();

                try
                {
                    co.ChangeDatabase(id);
                }
                catch (SqlException ex) // Schema doesn't exists, usually
                {
                    var message = string.Format("An error occured while retrieving '{0}' schema.", id);
                    AddFlashMessage(message, FlashMessageType.Error);
                    Logger.ErrorException(message, ex);

                    return RedirectToAction("Index");
                }

                var schem = await Task.Run(() => co.Query<SchemaInfo>(SchemaInfo.Query).ToList());
                var tList = schem.Select(_ => _.Table).Distinct();

                foreach (var t in tList)
                {
                    var __ = t;

                    var rc = await Task.Run(() => co.Query<int>(string.Format(SchemaInfo.RowCountQuery, __)).First());

                    foreach (var c in schem.Where(_ => _.Table == __))
                    {
                        c.RowCount = rc;
                    }
                }

                ViewBag.Database = id;
                return View(schem);
            }
        }

        /**
         * Backup entry points
         */

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Backup(string id)
        {
            var bk = await BackupsManager.BackupDatabase(
                GetBackupsConnectionString(),
                id,
                Logger
            );

            if (bk == null)
            {
                return HttpNotFound(string.Format("Unable to create database backup for '{0}'", id));
            }

            return PartialView("_BackupItem", bk);
        }

        // [HttpPost]
        public async Task<ActionResult> Backup_Fmt(string id, string format = "json")
        {
            var bk = await BackupsManager.BackupDatabase(
                GetBackupsConnectionString(),
                id,
                Logger
            );

            if (string.IsNullOrWhiteSpace(format))
            {
                return new HttpStatusCodeResult(406, "No format specified"); // Not acceptable
            }

            if (bk == null)
            {
                return HttpNotFound(string.Format("Unable to create database backup for '{0}'", id));
            }

            switch (format.ToLowerInvariant())
            {
                case "xml":
                    var t = new MemoryStream();
                    BackupHistoryXmlSerializer.Serialize(t, bk);
                    t.Position = 0;
                    return File(t, "application/xml");

                case "json":
                    return Json(bk, JsonRequestBehavior.AllowGet);

                case "zip":
                    var path     = bk.Path;
                    var fileName = Path.GetFileName(path);

                    return File(path, "application/zip", fileName);

                default:
                    return new HttpStatusCodeResult(406, "Unknown format");
            }
        }

        public ActionResult Download(int id)
        {
            var bk = DbContext.History.Find(id);

            if (bk == null)
            {
                return HttpNotFound();
            }

            var path     = bk.Path;
            var fileName = Path.GetFileName(path);

            return File(path, "application/zip", fileName);
        }

        /**
         * Deletion / Purge
         */

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
        public ActionResult BackupsPurge()
        {
            if (BackupsManager.PurgeOldBackups(DbContext, DateTime.Now, Logger))
            {
                AddFlashMessage("Outdated backups successfully purged", FlashMessageType.Success);
            }
            else
            {
                AddFlashMessage("An error occured during purging", FlashMessageType.Error);
            }

            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var bk = DbContext.History.Find(id);

            if (bk == null)
            {
                return HttpNotFound("Not a valid backup id");
            }

            if (BackupsManager.DeleteBackup(DbContext, bk, Logger))
            {
                try
                {
                    DbContext.SaveChanges();
                    if (Request.IsAjaxRequest())
                    {
                        return Content("OK", "text/plain");
                    }

                    AddFlashMessage("Backup successfully deleted", FlashMessageType.Success);

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Logger.ErrorException("While saving database changes", ex);
                }
            }

            if (Request.IsAjaxRequest())
            {
                return Content("ERR", "text/plain");
            }

            AddFlashMessage("An error occured while deleting backup.", FlashMessageType.Error);

            return RedirectToAction("Index");
        }
    }
}
