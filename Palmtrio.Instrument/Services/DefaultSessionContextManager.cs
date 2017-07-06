using Palmtrio.Domain.Interfaces;
using Palmtrio.Domain.Models;
using System.Web.SessionState;

namespace Palmtrio.Instrument.Services
{
    public class DefaultSessionContextManager : ISessionContextManager
    {
        private HttpSessionState _sessionState;

        public DefaultSessionContextManager(
            IHttpSessionStateWrapped InjContactSessionState)
        {
            _sessionState = InjContactSessionState.HttpSessionState;
        }

        public void PurgeSession()
        {
            var sess = new SessionContextModel();
            this.SaveSession(sess);
        }

        public void SetUserIp(string UserIp)
        {
            var sess = this.GetSession();
            sess.UserIp = UserIp;
            this.SaveSession(sess);
        }

        private SessionContextModel GetSession()
        {
            SessionContextModel kernel = _sessionState["kernel"] as SessionContextModel;
            if (kernel == null)
            {
                kernel = new SessionContextModel();
            }
            return kernel;
        }
        private void SaveSession(SessionContextModel Kernel)
        {
            _sessionState["kernel"] = Kernel;
        }

    }
}
