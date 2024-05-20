using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    bool TryMatchTitle(string title, [NotNullWhen(true)] out int? myAnimeListId);
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

    public bool TryMatchTitle(string title, out int? myAnimeListId)
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
            myAnimeListId = null;
            return false;
        }

        myAnimeListId = resource.MalId;
        return true;
    }

    public IEnumerable<string> GetTitles(int myAnimeListId)
    {
        var manga = GetManga(myAnimeListId);
        if (manga == null)
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
        return IsMatch(resource, title, x => x.HtmlDecode().ReplaceQuotations()) ||
               IsMatch(resource, title, x => x.HtmlDecode().ReplaceQuotations().StripNonAlphaNumeric());
    }

    private bool IsMatch(DataResource resource, string title, Func<string, string> titleTransform)
    {
        var replacedTitle = titleTransform(title);

        foreach (var resourceTitle in resource.Titles)
        {
            if (replacedTitle.EqualsIgnoreCase(titleTransform(resourceTitle.Title)))
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
