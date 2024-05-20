using System.Collections.Generic;
using System.Linq;
using System.Net;
using Indexarr.Core.Metadata.Jikan.Resource;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Metadata.Jikan.Result;

namespace NzbDrone.Core.Metadata.Jikan;

public interface IJikanService
{
    bool TryDirectMatchToTitleId(string title, out int myAnimeListId);
    IEnumerable<string> GetTitles(int myAnimeListId);
}

public class JikanService : MetadataSource, IJikanService
{
    private readonly IHttpRequestBuilderFactory _requestBuilder;

    public JikanService(IHttpClient httpClient, Logger logger, IJikanCloudRequestBuilder requestBuilder)
    : base(httpClient, logger)
    {
        _requestBuilder = requestBuilder.Services;
    }

    public bool TryDirectMatchToTitleId(string title, out int myAnimeListId)
    {
        var request = _requestBuilder.Create()
            .Resource("manga")
            .AddQueryParam("q", title)
            .Build();

        var response = ExecuteRequest(request);
        var data = Json.Deserialize<SearchMangaResult>(response.Content);
        var resource = data.Data.FirstOrDefault(x => IsMatch(x, title));
        if (resource == null)
        {
            myAnimeListId = -1;
            return false;
        }

        myAnimeListId = resource.Id;
        return true;
    }

    public IEnumerable<string> GetTitles(int myAnimeListId)
    {
        var manga = GetManga(myAnimeListId);
        if (manga == null)
        {
            yield break;
        }

        yield return manga.Data.Title;
        yield return manga.Data.TitleEnglish;
        yield return manga.Data.TitleJapanese;
        foreach (var titleResource in manga.Data.Titles)
        {
            yield return titleResource.Title;
        }
    }

    private bool IsMatch(DataResource resource, string title)
    {
        var replacedTitle = title.ReplaceQuotations();

        foreach (var resourceTitle in resource.Titles)
        {
            if (replacedTitle.EqualsIgnoreCase(resourceTitle.Title.HtmlDecode().ReplaceQuotations()))
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
