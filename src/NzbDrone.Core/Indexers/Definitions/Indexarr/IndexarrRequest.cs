using NzbDrone.Common.Http;

namespace NzbDrone.Core.Indexers.Definitions.Indexarr;

public class IndexarrRequest : IndexerRequest
{
    public bool IsTest { get; set; }

    public IndexarrRequest(string url, HttpAccept httpAccept)
        : base(url, httpAccept)
    {
    }

    public IndexarrRequest(HttpRequest httpRequest)
        : base(httpRequest)
    {
    }

    public IndexarrRequest(string url, HttpAccept httpAccept, bool isTest)
        : base(url, httpAccept)
    {
        IsTest = isTest;
    }

    public IndexarrRequest(HttpRequest httpRequest, bool isTest)
        : base(httpRequest)
    {
        IsTest = isTest;
    }
}
