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
            private set;
        }

        protected SSBTDbContext DbContext
        {
            get;
            private set;
        }

        protected override void Initialize(RequestContext requestContext)
        {
            var controllerName = requestContext.RouteData.Values["controller"] as string;
            var actionName     = requestContext.RouteData.Values["action"] as string;

            ViewBag.ControllerName = controllerName;
            Logger                 = LogManager.GetLogger(string.Format("{0}_{1}", controllerName, actionName));
            DbContext              = new SSBTDbContext();

            base.Initialize(requestContext);
        }

        protected override void Dispose(bool disposing)
        {
            DbContext.Dispose();
            base.Dispose(disposing);
        }
    }
}