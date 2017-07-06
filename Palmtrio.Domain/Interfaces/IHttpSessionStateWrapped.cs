using System.Web.SessionState;

namespace Palmtrio.Domain.Interfaces
{
    public interface IHttpSessionStateWrapped
    {
        HttpSessionState HttpSessionState { get; }
    }
}
