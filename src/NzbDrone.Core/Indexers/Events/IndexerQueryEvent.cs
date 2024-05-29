using System;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.Events
{
    public class IndexerQueryEvent : IEvent
    {
        public Guid IndexerId { get; set; }
        public SearchCriteriaBase Query { get; set; }
        public IndexerQueryResult QueryResult { get; set; }

        public IndexerQueryEvent(Guid indexerId, SearchCriteriaBase query, IndexerQueryResult result)
        {
            IndexerId = indexerId;
            Query = query;
            QueryResult = result;
        }
    }
}
