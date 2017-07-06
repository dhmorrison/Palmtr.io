using System;

namespace Palmtrio.Web.Framework.Filters
{
    public class RequireHttpsAttribute : System.Web.Mvc.RequireHttpsAttribute
    {
        protected override void HandleNonHttpsRequest(System.Web.Mvc.AuthorizationContext filterContext)
        {
            if(filterContext.HttpContext.Request.Url.Scheme != Uri.UriSchemeHttps)
            {
                var request = filterContext.HttpContext.Request;

                if (request.HttpMethod == "GET")
                {
                    base.HandleNonHttpsRequest(filterContext);
                }
                else
                {
                    filterContext.HttpContext.Response.Clear();
                    filterContext.HttpContext.Response.StatusCode = 404;
                }
            }
            else
            {
                base.HandleNonHttpsRequest(filterContext);
            }
        }
    }
}