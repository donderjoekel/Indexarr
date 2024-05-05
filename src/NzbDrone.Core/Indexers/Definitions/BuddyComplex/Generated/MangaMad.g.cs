// Auto generated

using System;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Indexers.Definitions.BuddyComplex;

public class MangaMad : BuddyComplexBase
{
    public MangaMad(IIndexerHttpClient httpClient, IEventAggregator eventAggregator, IIndexerStatusService indexerStatusService, IConfigService configService, Logger logger, IServiceProvider provider) : base(httpClient, eventAggregator, indexerStatusService, configService, logger, provider)
    {
    }

    public override string Name => "Manga Mad";

    public override string[] IndexerUrls => new[] { "https://mangamad.com/" };
}
