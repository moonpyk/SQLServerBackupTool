using System.Web.Mvc;
using System.Web.Routing;

namespace SQLServerBackupTool.Web
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Pseudo API routes

            routes.MapRoute(
                name: "BackupFmt",
                url: "api/Backup/{id}.{format}",
                defaults: new { controller="Home", action = "Backup_Fmt", }
            );

            routes.MapRoute(
                name: "ListFmt",
                url: "api/List.{format}",
                defaults: new { controller = "Home", action = "List_Fmt", }
            );

            // Backup route

            routes.MapRoute(
                name: "Backup",
                url: "Backup/{id}",
                defaults: new { controller = "Home", action= "Backup", }
            );

            // Default route

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional, }
            );
        }
    }
}