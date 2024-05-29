using System;
using System.Collections.Generic;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.Applications
{
    public interface IAppIndexerMapService
    {
        List<AppIndexerMap> GetMappingsForApp(Guid appId);
        AppIndexerMap Insert(AppIndexerMap appIndexerMap);
        AppIndexerMap Update(AppIndexerMap appIndexerMap);
        void Delete(Guid mappingId);
        void DeleteAllForApp(Guid appId);
    }

    public class AppIndexerMapService : IAppIndexerMapService, IHandle<ProviderDeletedEvent<IApplication>>
    {
        private readonly IAppIndexerMapRepository _appIndexerMapRepository;

        public AppIndexerMapService(IAppIndexerMapRepository appIndexerMapRepository)
        {
            _appIndexerMapRepository = appIndexerMapRepository;
        }

        public void DeleteAllForApp(Guid appId)
        {
            _appIndexerMapRepository.DeleteAllForApp(appId);
        }

        public void Delete(Guid mappingId)
        {
            _appIndexerMapRepository.Delete(mappingId);
        }

        public List<AppIndexerMap> GetMappingsForApp(Guid appId)
        {
            return _appIndexerMapRepository.GetMappingsForApp(appId);
        }

        public AppIndexerMap Insert(AppIndexerMap appIndexerMap)
        {
            return _appIndexerMapRepository.Insert(appIndexerMap);
        }

        public AppIndexerMap Update(AppIndexerMap appIndexerMap)
        {
            return _appIndexerMapRepository.Update(appIndexerMap);
        }

        public void Handle(ProviderDeletedEvent<IApplication> message)
        {
            _appIndexerMapRepository.DeleteAllForApp(message.ProviderId);
        }
    }
}
