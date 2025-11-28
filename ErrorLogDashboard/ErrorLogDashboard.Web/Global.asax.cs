using System;
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

        /// <summary>
        /// Global error handler for unhandled exceptions
        /// </summary>
        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            
            // Log the error (in a production environment, use a proper logging framework)
            System.Diagnostics.Debug.WriteLine($"Application Error: {exception?.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack Trace: {exception?.StackTrace}");
            
            // Clear the error and handle it gracefully
            Server.ClearError();
            
            // For API requests, return JSON error response
            var httpContext = HttpContext.Current;
            if (httpContext != null && httpContext.Request.Path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
            {
                httpContext.Response.Clear();
                httpContext.Response.StatusCode = 500;
                httpContext.Response.ContentType = "application/json";
                httpContext.Response.Write("{\"error\":\"An internal server error occurred.\"}");
                httpContext.Response.End();
                return;
            }
            
            // For MVC requests, redirect to error page
            Response.Redirect("~/Home/Index");
        }
    }
}
