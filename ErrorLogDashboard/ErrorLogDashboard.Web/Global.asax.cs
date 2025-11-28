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

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            
            // Log the error (in production, use a proper logging framework)
            System.Diagnostics.Debug.WriteLine($"Unhandled exception: {exception?.Message}");
            
            // Clear the error
            Server.ClearError();
            
            // For API requests, return a JSON error
            if (Context.Request.Path.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
            {
                Context.Response.StatusCode = 500;
                Context.Response.ContentType = "application/json";
                Context.Response.Write("{\"error\":\"An unexpected error occurred.\"}");
                return;
            }
            
            // For MVC requests, redirect to error page
            Response.Redirect("~/Home/Error");
        }
    }
}
