using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.IndexerSearch
{
    public interface IReleaseSearchService
    {
        Task<NewznabResults> Search(NewznabRequest request, List<int> indexerIds, bool interactiveSearch);
    }

    public class ReleaseSearchService : IReleaseSearchService
    {
        private readonly IIndexerLimitService _indexerLimitService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IIndexerFactory _indexerFactory;
        private readonly Logger _logger;

        public ReleaseSearchService(IEventAggregator eventAggregator,
                                IIndexerFactory indexerFactory,
                                IIndexerLimitService indexerLimitService,
                                Logger logger)
        {
            _eventAggregator = eventAggregator;
            _indexerFactory = indexerFactory;
            _indexerLimitService = indexerLimitService;
            _logger = logger;
        }

        public Task<NewznabResults> Search(NewznabRequest request, List<int> indexerIds, bool interactiveSearch)
        {
            throw new NotImplementedException();
        }
    }
}
