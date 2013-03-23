using System.Web.Mvc;
using System.Web.Routing;

namespace SQLServerBackupTool.Web.Lib.Mvc
{
    [Authorize]
    public class ApplicationController : Controller
    {
        protected override void Initialize(RequestContext requestContext)
        {
            ViewBag.ControllerName = requestContext.RouteData.Values["controller"] as string;

            base.Initialize(requestContext);
        }
    }
}