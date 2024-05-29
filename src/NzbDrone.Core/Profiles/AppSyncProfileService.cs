using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Profiles
{
    public interface IAppProfileService
    {
        AppSyncProfile Add(AppSyncProfile profile);
        void Update(AppSyncProfile profile);
        void Delete(Guid id);
        List<AppSyncProfile> All();
        AppSyncProfile Get(Guid id);
        bool Exists(Guid id);
        AppSyncProfile GetDefaultProfile(string name);
    }

    public class AppSyncProfileService : IAppProfileService,
        IHandle<ApplicationStartedEvent>
    {
        private readonly IAppProfileRepository _profileRepository;
        private readonly IIndexerFactory _indexerFactory;
        private readonly Logger _logger;

        public AppSyncProfileService(IAppProfileRepository profileRepository,
                              IIndexerFactory indexerFactory,
                              Logger logger)
        {
            _profileRepository = profileRepository;
            _indexerFactory = indexerFactory;
            _logger = logger;
        }

        public AppSyncProfile Add(AppSyncProfile profile)
        {
            return _profileRepository.Insert(profile);
        }

        public void Update(AppSyncProfile profile)
        {
            _profileRepository.Update(profile);
        }

        public void Delete(Guid id)
        {
            _profileRepository.Delete(id);
        }

        public List<AppSyncProfile> All()
        {
            return _profileRepository.All().ToList();
        }

        public AppSyncProfile Get(Guid id)
        {
            return _profileRepository.Get(id);
        }

        public bool Exists(Guid id)
        {
            return _profileRepository.Exists(id);
        }

        public void Handle(ApplicationStartedEvent message)
        {
            if (All().Any())
            {
                return;
            }

            _logger.Info("Setting up default app profile");

            AddDefaultProfile("Standard");
        }

        public AppSyncProfile GetDefaultProfile(string name)
        {
            var qualityProfile = new AppSyncProfile
            {
                Name = name,
                EnableRss = true,
                EnableAutomaticSearch = true,
                EnableInteractiveSearch = true,
                MinimumSeeders = 1
            };

            return qualityProfile;
        }

        private AppSyncProfile AddDefaultProfile(string name)
        {
            var profile = GetDefaultProfile(name);

            return Add(profile);
        }
    }
}
