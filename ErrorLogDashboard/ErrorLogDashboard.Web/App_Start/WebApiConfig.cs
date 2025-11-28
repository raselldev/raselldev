using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ErrorLogDashboard.Web
{
    /// <summary>
    /// Web API configuration
    /// </summary>
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Enable attribute routing
            config.MapHttpAttributeRoutes();

            // Convention-based routing
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Configure JSON serialization
            var jsonSettings = config.Formatters.JsonFormatter.SerializerSettings;
            jsonSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            jsonSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss";
            jsonSettings.NullValueHandling = NullValueHandling.Ignore;
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            // Remove XML formatter (JSON only)
            config.Formatters.Remove(config.Formatters.XmlFormatter);
        }
    }
}
