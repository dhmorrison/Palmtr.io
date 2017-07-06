using System.Web;
using Palmtrio.Domain.Interfaces;

namespace Palmtrio.Web.Framework.Session
{
    public class DefaultSessionState : IHttpSessionStateWrapped
    {
        public System.Web.SessionState.HttpSessionState HttpSessionState
        {
            get { return HttpContext.Current.Session; }
        }
    }
}