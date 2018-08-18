using System.Linq;
using System.Web.Http;
using Microsoft.Owin.Security.OAuth;
using System.Web.Http.Cors;
using WebApiContrib.Formatting.Jsonp;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace SalesForceOAuth
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            //var cors = new EnableCorsAttribute(origins: "http://www-dev0.websitealive.com,https://dev0.websitealive.com,http://localhost:56786,", headers: "*", methods: "*"); 
            //config.EnableCors(cors);
            var cors = new EnableCorsAttribute(origins: "*", headers: "*", methods: "*");
            config.EnableCors(cors);


            var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().FirstOrDefault();
            // Create Jsonp formatter
            var jsonpFormatter = new JsonpMediaTypeFormatter(jsonFormatter);
            // Add jsonp to the formatters list
            // config.Formatters.Add(jsonpFormatter);
            config.Formatters.Insert(0, jsonpFormatter);


            //  Add json formater
            config.Formatters.JsonFormatter.MediaTypeMappings.Add(
                new QueryStringMapping("type", "json", new MediaTypeHeaderValue("application/json")));


            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional, format = RouteParameter.Optional }
            );
        }
    }
}
