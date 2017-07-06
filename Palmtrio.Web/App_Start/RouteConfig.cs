using System.Web.Mvc;
using System.Web.Routing;

namespace Palmtrio.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");


#if DEBUG
            routes.MapRoute("StyleGuideRoute", "StyleGuide",
                new { controller = "Landing", action = "ViewStyleGuide" }
                );
#endif

            routes.MapRoute("LandingRoute", "",
                new { controller = "Landing", action = "Index" }
                );

            routes.MapRoute("LandingActionsRoute", "{action}",
                new { controller = "Landing", TeamDetailId = UrlParameter.Optional },
                new { action = "view-contact|view-team|view-team-detail|contact-submit" }
                );

            routes.MapRoute("LandingActionTeamDetailRoute", "{action}/{TeamDetailId}",
                new { controller = "Landing", action = "view-team-detail" },
                new { TeamDetailId = "1|2|3" }
                );

            routes.MapRoute("NotFoundRoute", "{*catchall}",
                new { controller = "Error", action = "NotFound" });
        }
    }
}
