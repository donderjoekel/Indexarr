using NzbDrone.Common.Http;

namespace NzbDrone.Core.Indexers.Definitions.Indexarr;

public class IndexarrResponse : IndexerResponse
{
    private readonly IndexarrRequest _request;

    public IndexarrResponse(IndexarrRequest indexerRequest, HttpResponse httpResponse)
        : base(indexerRequest, httpResponse)
    {
        _request = indexerRequest;
    }

    public new IndexarrRequest Request => _request;
}
