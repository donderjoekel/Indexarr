using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexerStatusService : IProviderStatusServiceBase<IndexerStatus>
    {
        ReleaseInfo GetLastRssSyncReleaseInfo(Guid indexerId);
        IDictionary<string, string> GetIndexerCookies(Guid indexerId);
        DateTime GetIndexerCookiesExpirationDate(Guid indexerId);

        void UpdateRssSyncStatus(Guid indexerId, ReleaseInfo releaseInfo);
        void UpdateCookies(Guid indexerId, IDictionary<string, string> cookies, DateTime? expiration);
    }

    public class IndexerStatusService : ProviderStatusServiceBase<IIndexer, IndexerStatus>, IIndexerStatusService
    {
        public IndexerStatusService(IIndexerStatusRepository providerStatusRepository, IEventAggregator eventAggregator, IRuntimeInfo runtimeInfo, Logger logger)
            : base(providerStatusRepository, eventAggregator, runtimeInfo, logger)
        {
        }

        public ReleaseInfo GetLastRssSyncReleaseInfo(Guid indexerId)
        {
            return GetProviderStatus(indexerId).LastRssSyncReleaseInfo;
        }

        public IDictionary<string, string> GetIndexerCookies(Guid indexerId)
        {
            return GetProviderStatus(indexerId)?.Cookies ?? null;
        }

        public DateTime GetIndexerCookiesExpirationDate(Guid indexerId)
        {
            return GetProviderStatus(indexerId)?.CookiesExpirationDate ?? DateTime.Now.AddDays(12);
        }

        public void UpdateRssSyncStatus(Guid indexerId, ReleaseInfo releaseInfo)
        {
            lock (_syncRoot)
            {
                var status = GetProviderStatus(indexerId);

                status.LastRssSyncReleaseInfo = releaseInfo;

                _providerStatusRepository.Upsert(status);
            }
        }

        public void UpdateCookies(Guid indexerId, IDictionary<string, string> cookies, DateTime? expiration)
        {
            if (indexerId != Guid.Empty)
            {
                lock (_syncRoot)
                {
                    var status = GetProviderStatus(indexerId);
                    status.Cookies = cookies;
                    status.CookiesExpirationDate = expiration;
                    _providerStatusRepository.Upsert(status);
                }
            }
        }
    }
}
