using System;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers.Definitions.Indexarr;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Indexers.Definitions.Madara;

public abstract class MadaraBase : IndexarrBase<MadaraRequestGenerator, MadaraResponseParser>
{
    public MadaraBase(IIndexerHttpClient httpClient, IEventAggregator eventAggregator, IIndexerStatusService indexerStatusService, IConfigService configService, Logger logger, IServiceProvider provider)
        : base(httpClient, eventAggregator, indexerStatusService, configService, logger, provider)
    {
    }

    public virtual int ChapterMode => 0;
    public override TimeSpan RateLimit => TimeSpan.Zero;
}
