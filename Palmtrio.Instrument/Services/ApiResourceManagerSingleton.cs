using Palmtrio.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;


namespace Palmtrio.Instrument.Services
{
    public class ApiResourceManagerSingleton : IApiResourceManager
    {
        private HashSet<Guid> _pendingResource = new HashSet<Guid>();
        private ConcurrentDictionary<Guid, ConcurrentQueue<IApiResourceHandle>> _assignedResource = new ConcurrentDictionary<Guid, ConcurrentQueue<IApiResourceHandle>>();

        private object _backlogLock = new object();

        public void RouteApiResource(Guid ResourceId)
        {
            _pendingResource.Add(ResourceId);
        }

        public void AssignApiResource(Guid ResourceId, IApiResourceHandle ApiResourceHandle)
        {
            if(_pendingResource.Contains(ResourceId))
            {
                if(!_assignedResource.ContainsKey(ResourceId))
                {
                    lock(_backlogLock)
                    {
                        if (!_assignedResource.ContainsKey(ResourceId))
                        {
                            _assignedResource[ResourceId] = new ConcurrentQueue<IApiResourceHandle>();
                        }
                    }
                }
                _assignedResource[ResourceId].Enqueue(ApiResourceHandle);
            }
        }

        public void RetractApiResource(Guid ResourceId)
        {
            if (_pendingResource.Contains(ResourceId))
            {
                _pendingResource.Remove(ResourceId);
            }

            ConcurrentQueue<IApiResourceHandle> backlogQueue;
            if(_assignedResource.TryRemove(ResourceId, out backlogQueue))
            {
                IApiResourceHandle videoStreamHandler;
                while (backlogQueue.TryDequeue(out videoStreamHandler))
                {
                    videoStreamHandler.Abort();
                }
            }
        }

        public bool HasPending(Guid ResourceId)
        {
            return _pendingResource.Contains(ResourceId);
        }

        public bool Exists(Guid ResourceId)
        {
            return this.HasPending(ResourceId) || _assignedResource.ContainsKey(ResourceId);
        }

    }
}
