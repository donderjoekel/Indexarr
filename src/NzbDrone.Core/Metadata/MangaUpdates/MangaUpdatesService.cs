using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Http.CloudFlare;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.MetadataSource.MangaUpdates.Resource.SeriesSearch;
using NzbDrone.Core.MetadataSource.MangaUpdates.Result;
using Polly;
using Polly.Retry;

namespace NzbDrone.Core.Metadata.MangaUpdates;

public interface IMangaUpdatesService
{
    bool TryDirectMatchTitleToId(string title, out long mangaUpdatesId);
    IEnumerable<string> GetTitles(long mangaUpdatesId);
}

public class MangaUpdatesService : IMangaUpdatesService
{
    private readonly IHttpClient _httpClient;
    private readonly Logger _logger;
    private readonly IHttpRequestBuilderFactory _requestBuilder;

    private ResiliencePipeline<HttpResponse> RetryStrategy => new ResiliencePipelineBuilder<HttpResponse>()
        .AddRetry(new RetryStrategyOptions<HttpResponse>
        {
            ShouldHandle = static args => args.Outcome switch
            {
                { Result.HasHttpServerError: true } => PredicateResult.True(),
                { Result.StatusCode: HttpStatusCode.RequestTimeout } => PredicateResult.True(),
                { Exception: HttpException { Response.HasHttpServerError: true } } => PredicateResult.True(),
                _ => PredicateResult.False()
            },
            Delay = TimeSpan.FromSeconds(1),
            MaxRetryAttempts = 2,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            OnRetry = args =>
            {
                var exception = args.Outcome.Exception;

                if (exception is not null)
                {
                    _logger.Warn(exception,
                        "Request for '{0}' failed with exception '{1}'. Retrying in {2}s.",
                        args.Outcome.Result?.Request.Url.ToString() ?? "MangaUpdatesService",
                        exception.Message,
                        args.RetryDelay.TotalSeconds);
                }
                else
                {
                    _logger.Warn("Request for '{0}' failed with status {1}. Retrying in {2}s.",
                        args.Outcome.Result?.Request.Url.ToString() ?? "MangaUpdatesService",
                        args.Outcome.Result?.StatusCode,
                        args.RetryDelay.TotalSeconds);
                }

                return default;
            }
        })
        .Build();

    public MangaUpdatesService(
        IHttpClient httpClient,
        Logger logger,
        IMangaUpdatesCloudRequestBuilder requestBuilder)
    {
        _httpClient = httpClient;
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

    private async Task<Response> ExecuteRequestAsync(HttpRequest request)
    {
        _logger.Debug("Downloading Feed " + request.ToString(false));

        // TODO: possibly disable this
        request.LogResponseContent = true;

        var originalUrl = request.Url;

        /*Cookies = GetCookies();

        if (Cookies != null)
        {
            foreach (var cookie in Cookies)
            {
                request.Cookies.Add(cookie.Key, cookie.Value);
            }
        }*/

        request.SuppressHttpError = true;

        var response = await RetryStrategy
            .ExecuteAsync(
                static async (state, _) => await state._httpClient.ExecuteAsync(state.request),
                (_httpClient, request))
            .ConfigureAwait(false);

        if (CloudFlareDetectionService.IsCloudflareProtected(response))
        {
            throw new CloudFlareProtectionException(response);
        }

        // Throw common http errors here before we try to parse
        if (response.HasHttpError && (request.SuppressHttpErrorStatusCodes == null ||
                                      !request.SuppressHttpErrorStatusCodes.Contains(response.StatusCode)))
        {
            if (response.Request.LogHttpError)
            {
                _logger.Warn("HTTP Error - {0}", response);
            }

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new TooManyRequestsException(request, response);
            }

            if (response.HasHttpServerError)
            {
                throw new HttpException(request, response);
            }
        }

        /*UpdateCookies(request.Cookies, DateTime.Now.AddDays(30));*/

        return new Response(request, response);
    }
}
