using System.Web.Mvc;
using System.Web.Routing;
using NLog;

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

        protected override void Initialize(RequestContext requestContext)
        {
            var controllerName = requestContext.RouteData.Values["controller"] as string;
            var actionName = requestContext.RouteData.Values["action"] as string;

            ViewBag.ControllerName = controllerName;
            Logger = LogManager.GetLogger(string.Format("{0}_{1}", controllerName, actionName));

            base.Initialize(requestContext);
        }
    }
}