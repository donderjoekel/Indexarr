using NzbDrone.Common.Http;

namespace NzbDrone.Core.Indexers
{
    public class IndexerResponse
    {
        private readonly IndexerRequest _indexerRequest;
        private readonly HttpResponse _httpResponse;

        public IndexerResponse(IndexerRequest indexerRequest, HttpResponse httpResponse)
        {
            _indexerRequest = indexerRequest;
            _httpResponse = httpResponse;
        }

        public IndexerRequest Request => _indexerRequest;

        public HttpRequest HttpRequest => _httpResponse.Request;

        public HttpResponse HttpResponse => _httpResponse;

        public string Content => _httpResponse.Content;
    }

    public class Response
    {
        private readonly HttpRequest _httpRequest;
        private readonly HttpResponse _httpResponse;

        public Response(HttpRequest httpRequest, HttpResponse httpResponse)
        {
            _httpRequest = httpRequest;
            _httpResponse = httpResponse;
        }

        public HttpRequest HttpRequest => _httpRequest;
        public HttpResponse HttpResponse => _httpResponse;
        public string Content => _httpResponse.Content;
    }
}
