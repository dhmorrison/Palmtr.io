using System;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Palmtrio.Web.Framework.Api
{
    public class RequireHttpsAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext.Request.RequestUri.Scheme != Uri.UriSchemeHttps)
            {
                var request = actionContext.Request;
                HttpResponseMessage response;

                if(request.Method.Equals(HttpMethod.Get))
                {
                    response = request.CreateResponse(System.Net.HttpStatusCode.Found);

                    UriBuilder uri = new UriBuilder(request.RequestUri);
                    uri.Scheme = Uri.UriSchemeHttps;
                    uri.Port = 443;

                    response.Headers.Location = uri.Uri;
                    
                    var cacheControlHeader = new System.Net.Http.Headers.CacheControlHeaderValue();
                    cacheControlHeader.Private = true;
                    response.Headers.CacheControl = cacheControlHeader;

                    string body = "<html><head><title>Object moved</title></head><body>\r<h2>Object moved to <a href=\""
                        + uri.Uri.AbsoluteUri
                        + "\">here</a>.</h2>\r</body></html>";

                    response.Content = new StringContent(body, System.Text.Encoding.UTF8, "text/html");
                }
                else
                {
                    response = request.CreateResponse(System.Net.HttpStatusCode.NotFound);

                    var cacheControlHeader = new System.Net.Http.Headers.CacheControlHeaderValue();
                    cacheControlHeader.Private = true;
                    response.Headers.CacheControl = cacheControlHeader;

                    response.Headers.Pragma.Clear();

                    string body = "<!DOCTYPE html><html><head><title>404</title></head><body>Not Found</body></html>";

                    response.Content = new StringContent(body, System.Text.Encoding.UTF8, "text/html");
                }


                actionContext.Response = response;

            }
            else
            {
                base.OnAuthorization(actionContext);
            }
        }

    }
}