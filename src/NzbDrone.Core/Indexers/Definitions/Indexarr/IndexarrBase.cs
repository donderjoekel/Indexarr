using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Reflection;

namespace NzbDrone.Core.Indexers.Definitions.Indexarr;

public abstract class IndexarrBase<TRequestGenerator, TResponseParser>
    : IndexarrBase<TRequestGenerator, TResponseParser, IndexarrBaseSettings>
    where TRequestGenerator : IndexarrRequestGenerator
    where TResponseParser : IndexarrResponseParser
{
    protected IndexarrBase(IIndexerHttpClient httpClient, IEventAggregator eventAggregator, IIndexerStatusService indexerStatusService, IConfigService configService, Logger logger, IServiceProvider provider)
        : base(httpClient, eventAggregator, indexerStatusService, configService, logger, provider)
    {
    }
}

public abstract class IndexarrBase<TRequestGenerator, TResponseParser, TSettings> : TorrentIndexerBase<IndexarrBaseSettings>
    where TRequestGenerator : IndexarrRequestGenerator
    where TResponseParser : IndexarrResponseParser
    where TSettings : IndexarrBaseSettings
{
    private readonly IServiceProvider _provider;

    public sealed override IndexerPrivacy Privacy => IndexerPrivacy.Public;

    public sealed override IndexerCapabilities Capabilities => GetCapabilities();

    public override string Description => string.Empty;

    protected virtual string ImageReferrer => Settings.BaseUrl;

    protected IndexarrBase(IIndexerHttpClient httpClient,
        IEventAggregator eventAggregator,
        IIndexerStatusService indexerStatusService,
        IConfigService configService,
        Logger logger,
        IServiceProvider provider)
        : base(httpClient,
            eventAggregator,
            indexerStatusService,
            configService,
            logger)
    {
        _provider = provider;
    }

    public sealed override IndexerCapabilities GetCapabilities()
    {
        var caps = new IndexerCapabilities()
        {
            TvSearchParams = new List<TvSearchParam>()
            {
                TvSearchParam.Q, TvSearchParam.Season, TvSearchParam.Ep
            },
        };

        var categories = GetCategories().ToList();

        if (!categories.Any())
        {
            throw new Exception("No categories defined for indexer");
        }

        foreach (var category in categories)
        {
            caps.Categories.AddCategoryMapping("Books", category, category.Name);
        }

        return caps;
    }

    protected virtual IEnumerable<IndexerCategory> GetCategories()
    {
        return new List<IndexerCategory>
        {
            NewznabStandardCategory.Books
        };
    }

    public sealed override IParseIndexerResponse GetParser()
    {
        return CreationHelper.Create<TResponseParser>(_provider, this, Definition, Settings, _logger);
    }

    public sealed override IIndexerRequestGenerator GetRequestGenerator()
    {
        return CreationHelper.Create<TRequestGenerator>(_provider, this, Definition, Settings, _logger);
    }
}
