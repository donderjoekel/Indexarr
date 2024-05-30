using System;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers.Definitions.Indexarr;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Indexers.Definitions.BuddyComplex;

public abstract class BuddyComplexBase : IndexarrBase<BuddyComplexRequestGenerator, BuddyComplexResponseParser>
{
    protected BuddyComplexBase(IIndexerHttpClient httpClient, IEventAggregator eventAggregator, IIndexerStatusService indexerStatusService, IConfigService configService, Logger logger, IServiceProvider provider)
        : base(httpClient, eventAggregator, indexerStatusService, configService, logger, provider)
    {
    }
}
