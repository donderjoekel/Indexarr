using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Http.CloudFlare;
using NzbDrone.Core.Indexers;
using Polly;
using Polly.Retry;

namespace NzbDrone.Core.Metadata;

public class MetadataSource
{
    private readonly IHttpClient _httpClient;
    private readonly Logger _logger;

    public MetadataSource(IHttpClient httpClient, Logger logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

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

    protected Response ExecuteRequest(HttpRequest request)
    {
        return ExecuteRequestAsync(request).GetAwaiter().GetResult();
    }

    protected async Task<Response> ExecuteRequestAsync(HttpRequest request)
    {
        _logger.Debug("Downloading Feed " + request.ToString(false));

        // TODO: possibly disable this
        request.LogResponseContent = true;

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

        return new Response(request, response);
    }
}
