using System;
using System.Collections.Generic;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.Definitions.Indexarr;

public interface IIndexarrRequestGenerator : IIndexerRequestGenerator
{
    // public IndexerPageableRequestChain GetFullIndexRequests();
    // public IndexerPageableRequestChain GetPartialIndexRequests();
}

public abstract class IndexarrRequestGenerator : IIndexarrRequestGenerator
{
    public IndexerPageableRequestChain GetSearchRequests(MovieSearchCriteria searchCriteria)
    {
        throw new NotImplementedException();
    }

    public IndexerPageableRequestChain GetSearchRequests(MusicSearchCriteria searchCriteria)
    {
        throw new NotImplementedException();
    }

    public IndexerPageableRequestChain GetSearchRequests(TvSearchCriteria searchCriteria)
    {
        throw new NotImplementedException();
    }

    public IndexerPageableRequestChain GetSearchRequests(BookSearchCriteria searchCriteria)
    {
        throw new NotImplementedException();
    }

    public IndexerPageableRequestChain GetSearchRequests(BasicSearchCriteria searchCriteria)
    {
        throw new NotImplementedException();
    }

    public IndexerPageableRequestChain GetFullIndexRequests()
    {
        var indexerPageableRequestChain = new IndexerPageableRequestChain();
        indexerPageableRequestChain.Add(new[] { GetFullIndexRequest() });
        return indexerPageableRequestChain;
    }

    public IndexerPageableRequestChain GetTestIndexRequests()
    {
        var indexerPageableRequestChain = new IndexerPageableRequestChain();
        indexerPageableRequestChain.Add(new[] { GetTestIndexRequest() });
        return indexerPageableRequestChain;
    }

    protected abstract IndexerRequest GetFullIndexRequest();
    protected abstract IndexerRequest GetTestIndexRequest();

    public Func<IDictionary<string, string>> GetCookies { get; set; }
    public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }
}
