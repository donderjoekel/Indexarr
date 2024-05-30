using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Concurrency;
using NzbDrone.Core.Indexers.Definitions.Indexarr;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers.Definitions.NepNep;

public class NepNepResponseParser : IndexarrResponseParser
{
    private readonly IIndexerHttpClient _httpClient;
    private readonly Logger _logger;
    private readonly NepNepBase _nepNepBase;

    public NepNepResponseParser(ProviderDefinition providerDefinition, IIndexerHttpClient httpClient, Logger logger, NepNepBase nepNepBase)
        : base(providerDefinition)
    {
        _httpClient = httpClient;
        _logger = logger;
        _nepNepBase = nepNepBase;
    }

    protected override IList<MangaInfo> ParseFullIndexResponse(HttpResponse response)
    {
        return ParseResponse(response, false);
    }

    protected override IList<MangaInfo> ParseTestIndexResponse(HttpResponse response)
    {
        return ParseResponse(response, true);
    }

    private List<MangaInfo> ParseResponse(HttpResponse response, bool isTest)
    {
        var mangas = new List<MangaInfo>();
        var match = Regex.Match(response.Content, @"(?=Directory =).+?(\[.+?\])\;");
        var json = match.Groups[1].Value;
        var directory = JsonConvert.DeserializeObject<List<DirectoryItem>>(json);
        if (isTest)
        {
            mangas = RunSequentially(directory, true);
        }
        else
        {
            mangas = RunConcurrently(directory);
        }

        return mangas;
    }

    private List<MangaInfo> RunSequentially(List<DirectoryItem> directory, bool isTest)
    {
        var mangas = new List<MangaInfo>();

        foreach (var item in directory)
        {
            try
            {
                var mangaUrl = Settings.BaseUrl + "manga/" + item.Index;
                var chapters = GetChapters(item, mangaUrl);
                mangas.Add(new MangaInfo()
                {
                    Title = item.Slug,
                    Url = mangaUrl,
                    Chapters = chapters
                });
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to parse manga info");
                continue;
            }

            if (isTest)
            {
                return mangas;
            }
        }

        return mangas;
    }

    private List<MangaInfo> RunConcurrently(List<DirectoryItem> directory)
    {
        using var semaphore = new SemaphoreSlim(25);
        var resetEvent = new AutoResetEvent(true);
        var mangas = new List<MangaInfo>();

        ConcurrentWork.CreateAndRun(25,
            directory,
            item => () =>
            {
                var mangaUrl = Settings.BaseUrl + "manga/" + item.Index;
                var chapters = GetChapters(item, mangaUrl);
                resetEvent.WaitOne();
                mangas.Add(new MangaInfo()
                {
                    Title = item.Slug,
                    Url = mangaUrl,
                    Chapters = chapters
                });
                resetEvent.Set();
            });

        return mangas;
    }

    private List<Parser.Model.ChapterInfo> GetChapters(DirectoryItem item, string mangaUrl)
    {
        _logger.Info("Requesting chapters for '{0}' from {1}", item.Slug, mangaUrl);
        var request = new HttpRequest(mangaUrl);
        var customResponse = _nepNepBase.ExecuteRequest(request);
        var match = Regex.Match(customResponse.Content, @"(?=Chapters =).+?(\[.+?\])\;", RegexOptions.IgnoreCase);
        var json = match.Groups[1].Value;
        var chapters = JsonConvert.DeserializeObject<List<ChapterInfo>>(json);
        if (chapters == null)
        {
            _logger.Error("No chapters found for '{0}' from {1}", item.Slug, mangaUrl);
            return new List<Parser.Model.ChapterInfo>();
        }

        var chapterInfos = new List<Parser.Model.ChapterInfo>();
        foreach (var chapter in chapters)
        {
            var chapterUrl = CreateUrl(Settings.BaseUrl, item.Index, chapter.Chapter, out var volume, out var chapterNumber);
            chapterInfos.Add(new Parser.Model.ChapterInfo()
            {
                Volume = volume,
                Number = chapterNumber,
                Url = chapterUrl,
                Date = DateTime.Parse(chapter.Date)
            });
        }

        return chapterInfos;
    }

    private string CreateUrl(string baseUrl, string indexName, string chapterCode, out int volume, out decimal chapterNumber)
    {
        volume = int.Parse(chapterCode[..1]);
        var index = volume != 1 ? "-index-" + volume : string.Empty;
        var n = int.Parse(chapterCode[1..^1]);
        var a = int.Parse(chapterCode[^1].ToString());
        var m = a != 0 ? "." + a : string.Empty;
        var id = indexName + "-chapter-" + n + m + index + ".html";
        chapterNumber = n + (a * 0.1m);
        var chapterUrl = baseUrl + "read-online/" + id;
        return chapterUrl;
    }

    private class DirectoryItem
    {
        [JsonProperty("i")]
        public string Index { get; set; }
        [JsonProperty("s")]
        public string Slug { get; set; }
        [JsonProperty("o")]
        public string Official { get; set; }
        [JsonProperty("ss")]
        public string ScanStatus { get; set; }
        [JsonProperty("ps")]
        public string PublishStatus { get; set; }
        [JsonProperty("t")]
        public string Type { get; set; }
        public string v { get; set; }
        public string vm { get; set; }
        [JsonProperty("y")]
        public string Year { get; set; }
        [JsonProperty("a")]
        public string[] Authors { get; set; }
        public string[] al { get; set; }
        [JsonProperty("l")]
        public string LatestChapter { get; set; }
        [JsonProperty("lt")]
        public long LastUpdated { get; set; }
        [JsonProperty("ls")]
        public string LastUpdatedString { get; set; }
        [JsonProperty("g")]
        public string[] Genres { get; set; }
        [JsonProperty("h")]
        public bool IsHot { get; set; }
    }

    private class ChapterInfo
    {
        public string Chapter { get; set; }
        public string Type { get; set; }
        public string Date { get; set; }
        public string ChapterName { get; set; }
        public string Page { get; set; }
        public string Directory { get; set; }
    }

    private class LatestRelease
    {
        [JsonProperty("SeriesID")]
        public string SeriesId { get; set; }
        public string IndexName { get; set; }
        public string SeriesName { get; set; }
        public string ScanStatus { get; set; }
        public string Chapter { get; set; }
        public string Genres { get; set; }
        public string Date { get; set; }
        public bool IsEdd { get; set; }
    }
}
