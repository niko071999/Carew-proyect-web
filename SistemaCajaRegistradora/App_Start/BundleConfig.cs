using System.Web;
using System.Web.Optimization;

namespace SistemaCajaRegistradora
{
    public class BundleConfig
    {
        // Para obtener más información sobre las uniones, visite https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            //CSS
            bundles.Add(new StyleBundle("~/Content/css-app").Include(
                      "~/Content/css/bootstrap.min.css",
                      "~/Content/css/sb-admin-2.min.css",
                      "~/Content/css/jquery.countdown.css",
                      "~/Scripts/vendor/fontawesome-free/css/all.min.css",
                      "~/Content/css/boleta.css"));

            bundles.Add(new StyleBundle("~/Content/css-app-sesion").Include(
                      "~/Content/css/bootstrap.min.css",
                      "~/Content/css/sb-admin-2.min.css",
                      "~/Scripts/vendor/fontawesome-free/css/all.min.css"));

            bundles.Add(new StyleBundle("~/Content/css-datatables").Include(
                        "~/Content/Datatable/datatables.min.css",
                        "~/Content/Datatable/dataTables.bootstrap5.min.css",
                        "~/Content/Datatable/responsive.bootstrap5.min.css",
                        "~/Content/Datatable/buttons.bootstrap5.min.css",
                        "~/Content/Datatable/dataTables.dateTime.min.css"));

            //JS
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-2.8.3.js"));

            bundles.Add(new Bundle("~/bundles/js-head").Include(
                      "~/Scripts/jquery-3.5.1.js",
                      "~/Scripts/jquery.plugin.min.js",
                      "~/Scripts/bootstrap.bundle.min.js",
                      "~/Scripts/jquery.countdown.min.js",
                      "~/Scripts/jquery.countdown-es.js"));

            bundles.Add(new Bundle("~/bundles/firebase").Include(
                        "~/Scripts/Firebase/firebase-app.js",
                        "~/Scripts/Firebase/firebase-storage.js"));

            bundles.Add(new Bundle("~/bundles/js-head-sesion").Include(
                      "~/Scripts/jquery-3.5.1.js",
                      "~/Scripts/bootstrap.bundle.min.js"));

            bundles.Add(new Bundle("~/bundles/js-login").Include(
                      "~/Scripts/js/sb-admin-2.min.js",
                      "~/Scripts/js/loginscripts.js"));

            bundles.Add(new Bundle("~/bundles/js-add-admin").Include(
                        "~/Scripts/js/sb-admin-2.min.js",
                        "~/Scripts/js/scriptsFirstOpen.js"));

            bundles.Add(new Bundle("~/bundles/app-scripts").Include(
                        "~/Scripts/js/sb-admin-2.min.js",
                        "~/Scripts/sweetalert2.11.js",
                        "~/Scripts/cleave.min.js",
                        "~/Scripts/JsBarcode.all.min.js",
                        "~/Scripts/js/alertmenssages.js",
                        "~/Scripts/js/usersesion.js",
                        "~/Scripts/js/scriptsUsuarios.js"));

            bundles.Add(new Bundle("~/bundles/js-datatables").Include(
                        "~/Scripts/Datatable/jquery.dataTables.min.js",
                        "~/Scripts/Datatable/datatables.min.js",
                        "~/Scripts/Datatable/dataTables.bootstrap5.min.js",
                        "~/Scripts/Datatable/responsive.bootstrap5.min.js",
                        "~/Scripts/Datatable/moment.min.js",
                        "~/Scripts/Datatable/dataTables.dateTime.min.js"));

            bundles.Add(new Bundle("~/bundles/js-dt-exports").Include(
                        "~/Scripts/Datatable/Exports/jszip.min.js",
                        "~/Scripts/Datatable/Exports/pdfmake.min.js",
                        "~/Scripts/Datatable/Exports/vfs_fonts.js",
                        "~/Scripts/Datatable/Exports/dataTables.buttons.min.js",
                        "~/Scripts/Datatable/Exports/buttons.bootstrap5.min.js",
                        "~/Scripts/Datatable/Exports/buttons.colVis.min.js",
                        "~/Scripts/Datatable/Exports/buttons.html5.min.js",
                        "~/Scripts/Datatable/Exports/buttons.print.min.jss"));

            BundleTable.EnableOptimizations = false;

        }
    }
}
