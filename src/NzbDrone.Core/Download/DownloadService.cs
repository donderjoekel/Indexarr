using System;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.TPL;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Download
{
    public interface IDownloadService
    {
        Task<byte[]> DownloadReport(string link, Guid indexerId, string source, string host, string title);
    }

    public class DownloadService : IDownloadService
    {
        private readonly IIndexerFactory _indexerFactory;
        private readonly IIndexerStatusService _indexerStatusService;
        private readonly IRateLimitService _rateLimitService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public DownloadService(IIndexerFactory indexerFactory,
                               IIndexerStatusService indexerStatusService,
                               IRateLimitService rateLimitService,
                               IEventAggregator eventAggregator,
                               Logger logger)
        {
            _indexerFactory = indexerFactory;
            _indexerStatusService = indexerStatusService;
            _rateLimitService = rateLimitService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public Task<byte[]> DownloadReport(string link, Guid indexerId, string source, string host, string title)
        {
            throw new NotImplementedException();
        }
    }
}
