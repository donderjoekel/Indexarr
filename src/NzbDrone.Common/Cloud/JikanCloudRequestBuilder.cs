using NzbDrone.Common.Http;

namespace NzbDrone.Common.Cloud;

public interface IJikanCloudRequestBuilder
{
    IHttpRequestBuilderFactory Services { get; }
}

public class JikanCloudRequestBuilder : IJikanCloudRequestBuilder
{
    public JikanCloudRequestBuilder()
    {
        Services = new HttpRequestBuilder("https://api.jikan.moe/v4/")
            .WithRateLimit(1)
            .SetHeader("Content-Type", "application/json")
            .SetHeader("Accept", "*/*")
            .SetHeader("Accept-Encoding", "gzip, deflate, br")
            .SetHeader("Connection", "keep-alive")
            .SetHeader("Host", "api.jikan.moe")
            .CreateFactory();
    }

    public IHttpRequestBuilderFactory Services { get; }
}
