using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace SQLServerBackupTool.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/ssbtw").Include(
                "~/Scripts/bootstrap.js",
                "~/Scripts/ssbtw.js"
            ));

            // Styles

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/Site.css"
            ));
        }
    }
}