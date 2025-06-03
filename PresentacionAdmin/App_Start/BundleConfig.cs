using System.Web.Optimization;

namespace PresentacionAdmin
{
    public class BundleConfig
    {
        // Para obtener más información sobre las uniones, visite https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Bundle para jQuery
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js"
            ));

            // Bundle para complementos (fontawesome, DataTables, etc.)
            bundles.Add(new ScriptBundle("~/bundles/complementos").Include(
                "~/Scripts/fontawesome/all.min.js",
                "~/Scripts/loadingoverlay/loadingoverlay.min.js",
                "~/Scripts/DataTables/jquery.dataTables.js",
                "~/Scripts/DataTables/dataTables.responsive.js",
                "~/Scripts/sweetalert.min.js",
                "~/Scripts/jquery.validate.js",
                "~/Scripts/jquery-ui.js",
                "~/Scripts/scripts.js"
            ));

            // Bundle para Bootstrap
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.bundle.js"
            ));

            // **Nuevo**: Bundle para Modernizr (si realmente lo usas)
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"
            ));

            // Bundle para estilos CSS
            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/site.css",
                "~/Content/breadcrumb.css",
                "~/Content/sweetalert.css",
                "~/Content/jquery-ui.css",
                "~/Content/DataTables/css/jquery.dataTables.css",
                "~/Content/DataTables/css/responsive.dataTables.css"
            ));
        }
    }
}
