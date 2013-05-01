using Dapper;
using SQLServerBackupTool.Lib.Annotations;
using SQLServerBackupTool.Web.Lib;
using SQLServerBackupTool.Web.Lib.Mvc;
using SQLServerBackupTool.Web.Models;
using SQLServerBackupTool.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Serialization;

namespace SQLServerBackupTool.Web.Controllers
{
    public class HomeController : ApplicationController
    {
        private static readonly Dictionary<Type, XmlSerializer> Serializers = new Dictionary<Type, XmlSerializer>();
        protected string MessageUnauthorizedDatabase;

        protected override void Initialize(RequestContext r)
        {
            base.Initialize(r);
            DatabaseSizeInfo.BindType();

            // We need a valid request context
            MessageUnauthorizedDatabase = __("You are not authorized to access this database");
        }

        /**
         * Index
         */

        public async Task<ActionResult> Index()
        {
            var bks = DbContext.History
                .AsEnumerable()
                .Where(_ => IsDatabaseAuthorized(User, _.Database))
                .OrderBy(_ => _.Id);

            var dbInfo = await BackupsManager.GetDatabasesInfo(User);

            return View(new IndexViewModel(dbInfo, bks));
        }

        /**
         * API Documentation entry point
         */

        public ActionResult ApiDoc()
        {
            return View();
        }

        /**
         * Database list as a service
         */

        public async Task<ActionResult> List_Fmt(string format = "json")
        {
            var dbInfo = await BackupsManager.GetDatabasesInfo(User);

            return FormatResult(format, dbInfo);
        }

        /**
         * Schema
         */

        public async Task<ActionResult> Schema(string id)
        {
            if (!IsDatabaseAuthorized(User, id))
            {
                AddFlashMessage(MessageUnauthorizedDatabase, FlashMessageType.Error);
                return RedirectToAction("Index");
            }

            using (var co = new SqlConnection(GetBackupsConnectionString()))
            {
                await co.OpenAsync();

                try
                {
                    co.ChangeDatabase(id);
                }
                catch (SqlException ex) // Schema doesn't exists, usually
                {
                    var message = string.Format(__("An error occurred while retrieving '{0}' schema."), id);
                    AddFlashMessage(message, FlashMessageType.Error);
                    Logger.ErrorException(message, ex);

                    return RedirectToAction("Index");
                }

                var sch = await Task.Run(() => co.Query<SchemaInfo>(SchemaInfo.Query).ToList());

                var tList = sch.Select(_ => _.Table).Distinct();

                foreach (var t in tList)
                {
                    var __ = t;

                    var rc = await Task.Run(() => co.Query<int>(string.Format(SchemaInfo.RowCountQuery, __)).First());

                    foreach (var c in sch.Where(_ => _.Table == __))
                    {
                        c.RowCount = rc;
                    }
                }

                ViewBag.Database = id;
                return View(sch);
            }
        }

        /**
         * Backup entry points
         */

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Backup(string id)
        {
            if (!IsDatabaseAuthorized(User, id))
            {
                AddFlashMessage(MessageUnauthorizedDatabase, FlashMessageType.Error);
                return RedirectToAction("Index");
            }

            var bk = await BackupsManager.BackupDatabase(
                GetBackupsConnectionString(),
                id,
                Logger
            );

            if (bk == null)
            {
                return HttpNotFound(string.Format(__("Unable to create database backup for '{0}'"), id));
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
                return HttpNotAcceptable(__("No format specified"));
            }

            if (!IsDatabaseAuthorized(User, id))
            {
                return HttpNotAuthorized();
            }

            if (bk == null)
            {
                return HttpNotFound(string.Format(__("Unable to create database backup for '{0}'"), id));
            }

            // Zip hook, direct backup and return result
            if (format.ToLowerInvariant() == @"zip")
            {
                var path = bk.Path;
                var fileName = Path.GetFileName(path);

                return File(path, "application/zip", fileName);
            }

            return FormatResult(format, bk);
        }

        public ActionResult Download(int id)
        {
            var bk = DbContext.History.Find(id);

            if (bk == null)
            {
                return HttpNotFound();
            }

            if (!IsDatabaseAuthorized(User, bk.Database))
            {
                return HttpNotAuthorized();
            }

            var path     = bk.Path;
            var fileName = Path.GetFileName(path);

            return File(path, "application/zip", fileName);
        }

        /**
         * Deletion / Purge
         */

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = @"Admin")]
        public ActionResult BackupsPurge()
        {
            if (BackupsManager.PurgeOldBackups(DbContext, DateTime.Now, Logger))
            {
                AddFlashMessage(__("Outdated backups successfully purged"), FlashMessageType.Success);
            }
            else
            {
                AddFlashMessage(__("An error occurred during purging"), FlashMessageType.Error);
            }

            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var bk = DbContext.History.Find(id);

            if (bk == null)
            {
                return HttpNotFound(__("Not a valid backup id"));
            }

            if (!User.IsInRole("Admin") && !User.IsInRole("Operator"))
            {
                var bkName = bk.Username;
                var uName  = User.Identity.Name;
                if (bkName == null || uName == null || (bkName.ToLowerInvariant() != User.Identity.Name.ToLowerInvariant()))
                {
                    return HttpNotAuthorized();
                }
            }

            if (BackupsManager.DeleteBackup(DbContext, bk, Logger))
            {
                try
                {
                    DbContext.SaveChanges();
                    if (Request.IsAjaxRequest())
                    {
                        return Content(@"OK", "text/plain");
                    }

                    AddFlashMessage(__("Backup successfully deleted"), FlashMessageType.Success);

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Logger.ErrorException(__("While saving database changes"), ex);
                }
            }

            if (Request.IsAjaxRequest())
            {
                return Content(@"ERR", "text/plain");
            }

            AddFlashMessage(__("An error occurred while deleting backup."), FlashMessageType.Error);

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Creates an API like result from a format and a given type 
        /// </summary>
        /// <typeparam name="T">Any type</typeparam>
        /// <param name="f">Format string</param>
        /// <param name="o">Object to serialize</param>
        /// <returns>
        /// An <see cref="ActionResult"/> depending of the asked format. 
        /// Returns <see cref="HttpStatusCodeResult"/> 406 if not valid format was asked for.
        /// </returns>
        protected ActionResult FormatResult<T>([NotNull] string f, T o)
        {
            if (string.IsNullOrWhiteSpace(f))
            {
                throw new ArgumentNullException("f");
            }

            switch (f.ToLowerInvariant())
            {
                case "xml":
                    var t = new MemoryStream();
                    var s = GetXmlSerializerFor(typeof(T));
                    s.Serialize(t, o);
                    t.Position = 0;
                    return File(t, "application/xml");

                case "json":
                    return Json(o, JsonRequestBehavior.AllowGet);

                default:
                    return HttpNotAcceptable(__("Unknown format"));
            }
        }

        /// <summary>
        /// Creates or get a cached version of an <see cref="XmlSerializer"/> for type <see cref="t"/>
        /// </summary>
        /// <param name="t">Type to create the <see cref="XmlSerializer"/> for</param>
        /// <returns>An <see cref="XmlSerializer"/> instance</returns>
        protected static XmlSerializer GetXmlSerializerFor(Type t)
        {
            if (Serializers.ContainsKey(t))
            {
                return Serializers[t];
            }

            var ret = new XmlSerializer(t);

            Serializers[t] = ret;

            return ret;
        }

        /// <summary>
        /// Is the current <see cref="user"/> authorized to access the database ?
        /// </summary>
        /// <param name="user"><see cref="IPrincipal"/> to test</param>
        /// <param name="database">Database name</param>
        /// <returns>true or false</returns>
        protected bool IsDatabaseAuthorized(IPrincipal user, string database)
        {
            return BackupsManager.IsDatabaseAuthorized(DbContext, user, database);
        }
    }
}
