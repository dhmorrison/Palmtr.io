using System.Web.Http;

namespace Palmtrio.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();
            //config.Filters.Add(new Palmtrio.Web.Framework.Api.RequireHttpsAttribute());
            
            config.Routes.MapHttpRoute(
                name: "VideoStreamApiRoute",
                routeTemplate: "api/vid/{vsid}/{speed}/{filename}.{extension}",
                defaults: new { controller = "VideoStreamApi", action = "GetVideoStream", speed = "fast" },
                constraints: new { extension = "mp4|ogg|webm", speed = "fast|slow" }
                );

        }
    }
}
