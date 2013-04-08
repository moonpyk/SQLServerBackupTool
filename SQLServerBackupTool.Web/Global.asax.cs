using NLog;
using NLog.Config;
using NLog.Targets;
using System.ComponentModel;
using System.IO;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace SQLServerBackupTool.Web
{
    // Remarque : pour obtenir des instructions sur l'activation du mode classique IIS6 ou IIS7, 
    // visitez http://go.microsoft.com/?LinkId=9394801
    [Localizable(false)]
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            // WebApiConfig.Register(GlobalConfiguration.Configuration);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ConfigureLogging(Server.MapPath("~"));
        }

        private static void ConfigureLogging(string rootPath)
        {
            var c = new LoggingConfiguration();

            var fileTarget = new FileTarget
            {
                FileName = Path.Combine(rootPath, "Logs", "${logger}.${shortdate}.log"),
            };

            fileTarget.Layout += "|${exception}";

            c.AddTarget("file", fileTarget);

            c.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, fileTarget));

            LogManager.Configuration = c;
        }
    }
}