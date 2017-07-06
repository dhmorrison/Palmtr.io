namespace Palmtrio.Domain.Interfaces
{
    public interface IPageHitCounter
    {
        void Count(string pageHitInfo = "");
    }
}
