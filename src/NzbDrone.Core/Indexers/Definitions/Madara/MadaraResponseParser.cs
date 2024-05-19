using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.Definitions.Indexarr;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers.Definitions.Madara;

public class MadaraResponseParser : IndexarrResponseParser
{
    private readonly IIndexerHttpClient _httpClient;
    private readonly MadaraBase _madaraBase;
    private readonly Logger _logger;

    public MadaraResponseParser(
        ProviderDefinition providerDefinition,
        IIndexerHttpClient httpClient,
        MadaraBase madaraBase,
        Logger logger)
        : base(providerDefinition)
    {
        _httpClient = httpClient;
        _madaraBase = madaraBase;
        _logger = logger;
    }

    protected override IList<MangaInfo> ParseFullIndexResponse(HttpResponse response)
    {
        var mangaInfos = ParseResponse(response, false);
        var page = 1;
        while (true)
        {
            var request = ((MadaraRequestGenerator)_madaraBase.GetRequestGenerator()).GetPageRequest(page);
            var customResponse = _madaraBase.ExecuteRequest(request.HttpRequest);
            var partialMangaInfos = ParseResponse(customResponse.HttpResponse, false);
            if (partialMangaInfos.Count == 0)
            {
                break;
            }

            mangaInfos.AddRange(partialMangaInfos);
            page++;
        }

        return mangaInfos;
    }

    protected override IList<MangaInfo> ParseTestIndexResponse(HttpResponse response)
    {
        return ParseResponse(response, true);
    }

    private List<MangaInfo> ParseResponse(HttpResponse response, bool isTest)
    {
        var mangaInfos = new List<MangaInfo>();
        var document = new HtmlParser().ParseDocument(response.Content);
        var elements = document.QuerySelectorAll<IHtmlDivElement>("div.page-item-detail");

        foreach (var element in elements)
        {
            try
            {
                var anchor = element.QuerySelector<IHtmlAnchorElement>(".post-title a");
                var title = anchor.TextContent.Trim();
                var url = anchor.Href;
                var chapters = GetChapters(url, title);
                mangaInfos.Add(new MangaInfo
                {
                    Title = title,
                    Url = url,
                    Chapters = chapters
                });
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to parse manga info");
                continue;
            }

            // We want to early out if it is a test parse because we don't want to unnecessarily parse too many items
            if (isTest)
            {
                return mangaInfos;
            }
        }

        return mangaInfos;
    }

    private List<ChapterInfo> GetChapters(string url, string title)
    {
        _logger.Debug("Requesting chapters for '{0}' from {1}", title, url);
        var request = GetChaptersRequest(url);
        var response = _madaraBase.ExecuteRequest(request);
        var mangaDocument = new HtmlParser().ParseDocument(response.Content);
        var chapters = new List<ChapterInfo>();
        var chapterElements = mangaDocument.QuerySelectorAll<IHtmlListItemElement>("li.wp-manga-chapter").ToList();
        if (!chapterElements.Any())
        {
            _logger.Error("No chapters found for '{0}' from {1}", title, url);
            return chapters;
        }

        foreach (var chapterElement in chapterElements)
        {
            var anchor = chapterElement.QuerySelector<IHtmlAnchorElement>("a");
            var chapterTitle = anchor.TextContent.Trim();
            if (!TryParseChapterNameToChapterNumber(chapterTitle, out var chapterNumber))
            {
                _logger.Warn("Unable to parse '{0}' as number for '{1}' from {2}", chapterTitle, title, url);
                continue;
            }

            var releaseDateElement = chapterElement.QuerySelector<IHtmlSpanElement>(".chapter-release-date");
            var parsedDate = DateTime.Today;
            if (releaseDateElement != null)
            {
                var releaseDate = releaseDateElement.TextContent.Trim();
                if (!TryParseDateTime(releaseDate, out parsedDate))
                {
                    _logger.Warn("Unable to parse '{0}' as date for '{1}' from {2}", releaseDate, title, url);
                }
            }
            else
            {
                _logger.Warn("No release date found for '{0}' from {1}", chapterTitle, url);
            }

            chapters.Add(new ChapterInfo()
            {
                Volume = 1,
                Url = anchor.Href,
                Date = parsedDate,
                Number = chapterNumber
            });
        }

        return chapters;
    }

    private HttpRequest GetChaptersRequest(string url)
    {
        HttpRequest request = null;

        switch (_madaraBase.ChapterMode)
        {
            case 0:
                request = new HttpRequest(url + "wp-admin/admin-ajax.php")
                {
                    Method = HttpMethod.Post
                };
                break;
            case 1:
                request = new HttpRequest(url + "ajax/chapters/")
                {
                    Method = HttpMethod.Post
                };
                break;
            case 2:
                request = new HttpRequest(url)
                {
                    Method = HttpMethod.Post
                };
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        request.Headers.ContentType = "application/x-www-form-urlencoded";
        return request;
    }
}
