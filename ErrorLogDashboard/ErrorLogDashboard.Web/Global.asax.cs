using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ErrorLogDashboard.Web
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // Register areas (if any)
            AreaRegistration.RegisterAllAreas();

            // Register Web API routes first
            GlobalConfiguration.Configure(WebApiConfig.Register);

            // Register MVC routes
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Register bundles
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
