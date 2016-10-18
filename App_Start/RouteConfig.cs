using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SalesForceOAuth
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "mynewTest",
                url: "api/SalesForce/GetAddNewLead/{ParamFirstName}",
                defaults: new { controller = "SalesForce", action = "GetAddNewLead", ParamFirstName = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default2",
                url: "api/{controller}/{action}/{id}",
                defaults: new { controller = "SalesForce", action = "get", id = UrlParameter.Optional }
            );

            //routes.MapRoute(
            //    name: "ControllerOnly",
            //    url: "api/{controller}",
            //    defaults: new {action = "GetApiInfo" }
            //);
            // Controllers with Actions
            // To handle routes like `/api/VTRouting/route`
            //routes.MapRoute(
            //    name: "ControllerAndAction",
            //    url: "api/{controller}/{action}"
            //);
        }
    }
}
