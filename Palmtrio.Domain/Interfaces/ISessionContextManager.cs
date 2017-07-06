namespace Palmtrio.Domain.Interfaces
{
    public interface ISessionContextManager
    {
        void PurgeSession();
        void SetUserIp(string UserIp);
    }
}
