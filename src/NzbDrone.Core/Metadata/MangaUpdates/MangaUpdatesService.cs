using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.MetadataSource.MangaUpdates.Resource.SeriesSearch;
using NzbDrone.Core.MetadataSource.MangaUpdates.Result;

namespace NzbDrone.Core.Metadata.MangaUpdates;

public interface IMangaUpdatesService
{
    bool TryDirectMatchTitleToId(string title, out long mangaUpdatesId);
    IEnumerable<string> GetTitles(long mangaUpdatesId);
}

public class MangaUpdatesService : MetadataSource, IMangaUpdatesService
{
    private readonly Logger _logger;
    private readonly IHttpRequestBuilderFactory _requestBuilder;

    public MangaUpdatesService(
        IHttpClient httpClient,
        Logger logger,
        IMangaUpdatesCloudRequestBuilder requestBuilder)
        : base(httpClient, logger)
    {
        _logger = logger;
        _requestBuilder = requestBuilder.Services;
    }

    public bool TryDirectMatchTitleToId(string title, out long mangaUpdatesId)
    {
        var httpRequest = _requestBuilder.Create()
            .WithRateLimit(1)
            .Resource("series/search")
            .AddBodyProperty("search", title)
            .AddBodyProperty("page", 1)
            .AddBodyProperty("perpage", 5)
            .Post()
            .Build();

        var response = ExecuteRequestAsync(httpRequest).GetAwaiter().GetResult();
        var data = Json.Deserialize<SeriesSearchResult>(response.Content);
        /*var httpResponse = _httpClient.Post<SeriesSearchResult>(httpRequest);*/
        var resource = data.Results.Where(Filter).FirstOrDefault(x => IsMatch(x, title));
        if (resource == null)
        {
            mangaUpdatesId = -1;
            return false;
        }

        mangaUpdatesId = resource.Record.SeriesId;
        return true;
    }

    public IEnumerable<string> GetTitles(long mangaUpdatesId)
    {
        var series = GetSeries(mangaUpdatesId);
        if (series == null)
        {
            yield break;
        }

        yield return series.Title;
        foreach (var resource in series.Associated)
        {
            yield return resource.Title;
        }
    }

    private bool IsMatch(SeriesSearchResultResource resource, string title)
    {
        var replacedTitle = title.ReplaceQuotations();

        if (replacedTitle.EqualsIgnoreCase(resource.HitTitle.HtmlDecode().ReplaceQuotations()))
        {
            return true;
        }

        if (replacedTitle.EqualsIgnoreCase(resource.Record.Title.HtmlDecode().ReplaceQuotations()))
        {
            return true;
        }

        SeriesGetResult series;
        try
        {
            series = GetSeries(resource.Record.SeriesId);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to get series details for {0}", resource.Record.SeriesId);
            return false;
        }

        if (series == null)
        {
            return false;
        }

        foreach (var associated in series.Associated)
        {
            if (replacedTitle.EqualsIgnoreCase(associated.Title.HtmlDecode().ReplaceQuotations()))
            {
                return true;
            }
        }

        return false;
    }

    private SeriesGetResult GetSeries(long mangaUpdatesId)
    {
        var httpRequest = _requestBuilder.Create()
            .WithRateLimit(1)
            .Resource($"series/{mangaUpdatesId}")
            .Build();

        var response = ExecuteRequestAsync(httpRequest).GetAwaiter().GetResult();
        if (response.HttpResponse.HasHttpError)
        {
            if (response.HttpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            else
            {
                throw new HttpException(httpRequest, response.HttpResponse);
            }
        }

        return Json.Deserialize<SeriesGetResult>(response.Content);
    }

    private bool Filter(SeriesSearchResultResource resource)
    {
        if (resource.Record.Type.EqualsIgnoreCase("doujinshi"))
        {
            return false;
        }

        if (resource.Record.Type.EqualsIgnoreCase("novel"))
        {
            return false;
        }

        foreach (var genreResource in resource.Record.Genres)
        {
            if (genreResource.Genre.EqualsIgnoreCase("adult"))
            {
                return false;
            }
        }

        return true;
    }
}
