using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace ErrorLogDashboard.Web
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            // Register MVC routes
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            
            // Register Web API routes
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
