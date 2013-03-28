using NLog;
using SQLServerBackupTool.Web.Models;
using System.Web.Mvc;
using System.Web.Routing;

namespace SQLServerBackupTool.Web.Lib.Mvc
{
    [Authorize]
    public class ApplicationController : Controller
    {
        protected Logger Logger
        {
            get;
            set;
        }

        protected SSBTDbContext DbContext
        {
            get;
            set;
        }

        protected override void Initialize(RequestContext r)
        {
            var controllerName = r.RouteData.GetRequiredString("controller");
            var actionName     = r.RouteData.GetRequiredString("action");

            ViewBag.ControllerName = controllerName;
            Logger                 = LogManager.GetLogger(string.Format("{0}_{1}", controllerName, actionName));
            DbContext              = new SSBTDbContext();

            base.Initialize(r);
        }

        protected override void Dispose(bool disposing)
        {
            DbContext.Dispose();
            base.Dispose(disposing);
        }
    }
}
