using System.Web.Optimization;

namespace ErrorLogDashboard.Web
{
    /// <summary>
    /// Bundle configuration for CSS and JavaScript files
    /// </summary>
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            // jQuery bundle
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js"));

            // Bootstrap bundle
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.bundle.js"));

            // Dashboard scripts bundle
            bundles.Add(new ScriptBundle("~/bundles/dashboard").Include(
                "~/Scripts/dashboard.js"));

            // Bootstrap CSS bundle
            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.css",
                "~/Content/dashboard.css"));
        }
    }
}
