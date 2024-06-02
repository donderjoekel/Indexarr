using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    bool TryMatchTitle(string title, [NotNullWhen(true)] out long? mangaUpdatesId);
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

    public bool TryMatchTitle(string title, out long? mangaUpdatesId)
    {
        try
        {
            var httpRequest = _requestBuilder.Create()
                .WithRateLimit(1)
                .Resource("series/search")
                .AddBodyProperty("search", title)
                .AddBodyProperty("page", 1)
                .AddBodyProperty("perpage", 5)
                .Post()
                .Build();

            var response = ExecuteRequest(httpRequest);
            if (response.HttpResponse.HasHttpError)
            {
                _logger.Error("Request failed with status code {StatusCode}", response.HttpResponse.StatusCode);
                mangaUpdatesId = null;
                return false;
            }

            var data = Json.Deserialize<SeriesSearchResult>(response.Content);
            var resource = data?.Results.Where(Filter).FirstOrDefault(x => IsMatch(x, title));
            if (resource == null)
            {
                mangaUpdatesId = null;
                return false;
            }

            mangaUpdatesId = resource.Record.SeriesId;
            return true;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to match title {Title}", title);
            mangaUpdatesId = null;
            return false;
        }
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
        if (CompareTitles(title, resource.HitTitle))
        {
            return true;
        }

        if (CompareTitles(title, resource.Record.Title))
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
            if (CompareTitles(title, associated.Title))
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

        return true;
    }
}
