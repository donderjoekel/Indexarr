using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.Metadata.Jikan;

public interface IJikanService
{
    bool TryDirectMatchToTitleId(string title, out int myAnimeListId);
    IEnumerable<string> GetTitles(int myAnimeListId);
}

public class JikanService : IJikanService
{
    private readonly IHttpClient _httpClient;
    private readonly Logger _logger;
    private readonly IHttpRequestBuilderFactory _requestBuilder;

    public JikanService(IHttpClient httpClient, Logger logger, IJikanCloudRequestBuilder requestBuilder)
    {
        _httpClient = httpClient;
        _logger = logger;
        _requestBuilder = requestBuilder.Services;
    }

    public bool TryDirectMatchToTitleId(string title, out int myAnimeListId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<string> GetTitles(int myAnimeListId)
    {
        throw new NotImplementedException();
    }
}
