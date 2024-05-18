// Auto generated

using System;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Indexers.Definitions.Madara;

public class NightComic : MadaraBase
{
    public NightComic(IIndexerHttpClient httpClient, IEventAggregator eventAggregator, IIndexerStatusService indexerStatusService, IConfigService configService, Logger logger, IServiceProvider provider)
        : base(httpClient, eventAggregator, indexerStatusService, configService, logger, provider)
    {
    }

    public override string Name => "Night Comic";
    public override string[] IndexerUrls => new[] { "https://www.nightcomic.com" };
    public override int ChapterMode => 1;
}
