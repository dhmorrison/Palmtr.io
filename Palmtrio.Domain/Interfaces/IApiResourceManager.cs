using System;

namespace Palmtrio.Domain.Interfaces
{
    public interface IApiResourceManager
    {
        void RouteApiResource(Guid ResourceId);

        void AssignApiResource(Guid ResourceId, IApiResourceHandle ApiResourceHandle);

        void RetractApiResource(Guid ResourceId);

        bool HasPending(Guid ResourceId);

        bool Exists(Guid ResourceId);
    }
}
