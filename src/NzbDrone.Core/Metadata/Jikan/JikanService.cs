using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using Indexarr.Core.Metadata.Jikan.Resource;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Metadata.Jikan.Result;

namespace NzbDrone.Core.Metadata.Jikan;

public interface IJikanService
{
    bool TryMatchTitle(string title, [NotNullWhen(true)] out int? myAnimeListId);
    IEnumerable<string> GetTitles(int myAnimeListId);
}

public class JikanService : MetadataSource, IJikanService
{
    private readonly IHttpRequestBuilderFactory _requestBuilder;
    private readonly Logger _logger;

    public JikanService(IHttpClient httpClient, Logger logger, IJikanCloudRequestBuilder requestBuilder)
    : base(httpClient, logger)
    {
        _logger = logger;
        _requestBuilder = requestBuilder.Services;
    }

    public bool TryMatchTitle(string title, out int? myAnimeListId)
    {
        try
        {
            var request = _requestBuilder.Create()
                .Resource("manga")
                .AddQueryParam("q", title)
                .Build();

            var response = ExecuteRequest(request);
            if (response.HttpResponse.HasHttpError)
            {
                _logger.Error("Request failed with status code {StatusCode}", response.HttpResponse.StatusCode);
                myAnimeListId = null;
                return false;
            }

            var data = Json.Deserialize<SearchMangaResult>(response.Content);
            var resource = data?.Data.FirstOrDefault(x => IsMatch(x, title));
            if (resource == null)
            {
                myAnimeListId = null;
                return false;
            }

            myAnimeListId = resource.MalId;
            return true;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to match title {Title}", title);
            myAnimeListId = null;
            return false;
        }
    }

    public IEnumerable<string> GetTitles(int myAnimeListId)
    {
        var manga = GetManga(myAnimeListId);

        if (manga?.Data == null)
        {
            yield break;
        }

        foreach (var titleResource in manga.Data.Titles)
        {
            yield return titleResource.Title;
        }
    }

    private bool IsMatch(DataResource resource, string title)
    {
        foreach (var resourceTitle in resource.Titles)
        {
            if (CompareTitles(title, resourceTitle.Title))
            {
                return true;
            }
        }

        return false;
    }

    private GetMangaResult GetManga(int myAnimeListId)
    {
        var request = _requestBuilder.Create()
            .Resource("manga/" + myAnimeListId)
            .Build();

        var response = ExecuteRequest(request);
        if (response.HttpResponse.HasHttpError)
        {
            if (response.HttpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            else
            {
                throw new HttpException(request, response.HttpResponse);
            }
        }

        return Json.Deserialize<GetMangaResult>(response.Content);
    }
}
