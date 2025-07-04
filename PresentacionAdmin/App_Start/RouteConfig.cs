﻿using System.Web.Mvc;
using System.Web.Routing;

namespace PresentacionAdmin
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Acceso", action = "Login", id = UrlParameter.Optional },
                namespaces: new[] { "PresentacionAdmin.Controllers" }
            );
        }
    }
}
