using System.ComponentModel;
using System.Web.Optimization;

namespace SQLServerBackupTool.Web
{
    [Localizable(false)]
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.validate.js",
                "~/Scripts/jquery.validate.unobtrusive.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/ssbtw").Include(
                "~/Scripts/bootstrap.js",
                "~/Scripts/jquery.pldr.js",
                "~/Scripts/select2.js",
                "~/Scripts/ssbtw.js"
            ).IncludeDirectory("~/Scripts/lang/", "*.js"));

            // Styles

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/Site.css"
            ));
        }
    }
}