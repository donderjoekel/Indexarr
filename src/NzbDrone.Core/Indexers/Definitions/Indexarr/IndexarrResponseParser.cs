using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers.Definitions.Indexarr;

public abstract class IndexarrResponseParser : IParseIndexerResponse
{
    private static readonly Regex InDaysRegex = new Regex(@"in (\d+) days", RegexOptions.IgnoreCase);
    private static readonly Regex InHoursRegex = new Regex(@"in (\d+) hours", RegexOptions.IgnoreCase);
    private static readonly Regex SecondsAgoRegex = new Regex(@"(\d+) seconds? ago", RegexOptions.IgnoreCase);
    private static readonly Regex MinutesAgoRegex = new Regex(@"(\d+) (?:minutes?|mins?) ago", RegexOptions.IgnoreCase);
    private static readonly Regex HoursAgoRegex = new Regex(@"(\d+) hours? ago", RegexOptions.IgnoreCase);
    private static readonly Regex DaysAgoRegex = new Regex(@"(\d+) days? ago", RegexOptions.IgnoreCase);
    private static readonly Regex MonthsAgoRegex = new Regex(@"(\d+) months? ago", RegexOptions.IgnoreCase);

    private static readonly Regex[] ChapterRegexes = new[]
    {
        /*
         * chapter 1
         * chapter - 1
         * chapter - 1.1
         */
        new Regex(@"chapter\s?-*\s*(\d+(\.\d+)?)", RegexOptions.IgnoreCase),
        /*
         * ch.1
         * ch.1.1
         * ch. 1
         * ch. 1.1
         */
        new Regex(@"ch.\s*(\d+(\.\d+)?)", RegexOptions.IgnoreCase),
        /*
         * ch-1
         * ch-1.1
         * ch - 1
         * ch - 1.1
         */
        new Regex(@"ch-\s*(\d+(\.\d+)?)", RegexOptions.IgnoreCase),
        /*
         * episode 1
         * episode - 1
         * episode - 1.1
         */
        new Regex(@"episode\s?-*\s*(\d+(\.\d+)?)", RegexOptions.IgnoreCase),
        /*
         * ch1
         * ch1.1
         * ch 1
         * ch 1.1
         */
        new Regex(@"ch\s*(\d+(\.\d+)?)", RegexOptions.IgnoreCase), // This one should always be last
    };

    private readonly ProviderDefinition _providerDefinition;

    public ProviderDefinition ProviderDefinition => _providerDefinition;
    public IndexarrBaseSettings Settings => (IndexarrBaseSettings)_providerDefinition.Settings;

    protected IndexarrResponseParser(ProviderDefinition providerDefinition)
    {
        _providerDefinition = providerDefinition;
    }

    public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }

    public IList<MangaInfo> ParseResponse(IndexerResponse indexerResponse)
    {
        if (indexerResponse == null)
        {
            throw new ArgumentNullException(nameof(indexerResponse));
        }

        if (indexerResponse.HttpResponse == null)
        {
            throw new ArgumentNullException(nameof(indexerResponse.HttpResponse));
        }

        if (indexerResponse.HttpResponse.Content == null)
        {
            throw new ArgumentNullException(nameof(indexerResponse.HttpResponse.Content));
        }

        if (indexerResponse.Request is not IndexarrRequest mangarrRequest)
        {
            throw new ArgumentException("Request must be of type MangarrRequest", nameof(indexerResponse.Request));
        }

        var parsed = new List<MangaInfo>();

        parsed.AddRange(mangarrRequest.IsTest
            ? ParseTestIndexResponse(indexerResponse.HttpResponse)
            : ParseFullIndexResponse(indexerResponse.HttpResponse));

        return parsed;
    }

    protected abstract IList<MangaInfo> ParseFullIndexResponse(HttpResponse response);

    protected abstract IList<MangaInfo> ParseTestIndexResponse(HttpResponse response);

    protected bool TryParseChapterNameToChapterNumber(string chapterName, out decimal chapterNumber)
    {
        if (decimal.TryParse(chapterName, out chapterNumber))
        {
            return true;
        }

        chapterNumber = -1;

        foreach (var chapterRegex in ChapterRegexes)
        {
            var result = chapterRegex.Match(chapterName).Groups[1].Value.Trim();
            if (decimal.TryParse(result, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out chapterNumber))
            {
                return true;
            }
        }

        return false;
    }

    protected bool TryParseDateTime(string input, out DateTime output)
    {
        if (DateTime.TryParse(input, out output))
        {
            return true;
        }

        output = ParseHumanReleaseDate(input);
        return output != DateTime.MinValue;
    }

    private static DateTime ParseHumanReleaseDate(string input)
    {
        if (InDaysRegex.IsMatch(input))
        {
            return DateTime.Now.AddDays(int.Parse(InDaysRegex.Match(input).Groups[1].Value));
        }

        if (string.Equals(input, "in a day", StringComparison.OrdinalIgnoreCase))
        {
            return DateTime.Now.AddDays(1);
        }

        if (InHoursRegex.IsMatch(input))
        {
            return DateTime.Now.AddHours(int.Parse(InHoursRegex.Match(input).Groups[1].Value));
        }

        if (string.Equals(input, "in an hour", StringComparison.OrdinalIgnoreCase))
        {
            return DateTime.Now.AddHours(1);
        }

        if (SecondsAgoRegex.IsMatch(input))
        {
            return DateTime.Now.AddSeconds(-int.Parse(SecondsAgoRegex.Match(input).Groups[1].Value));
        }

        if (MinutesAgoRegex.IsMatch(input))
        {
            return DateTime.Now.AddMinutes(-int.Parse(MinutesAgoRegex.Match(input).Groups[1].Value));
        }

        if (string.Equals(input, "an hour ago", StringComparison.OrdinalIgnoreCase))
        {
            return DateTime.Now.AddHours(-1);
        }

        if (HoursAgoRegex.IsMatch(input))
        {
            return DateTime.Now.AddHours(-int.Parse(HoursAgoRegex.Match(input).Groups[1].Value));
        }

        if (string.Equals(input, "a day ago", StringComparison.OrdinalIgnoreCase))
        {
            return DateTime.Now.AddDays(-1);
        }

        if (DaysAgoRegex.IsMatch(input))
        {
            return DateTime.Now.AddDays(-int.Parse(DaysAgoRegex.Match(input).Groups[1].Value));
        }

        if (string.Equals(input, "a month ago", StringComparison.OrdinalIgnoreCase))
        {
            return DateTime.Now.AddMonths(-1);
        }

        if (MonthsAgoRegex.IsMatch(input))
        {
            return DateTime.Now.AddMonths(-int.Parse(MonthsAgoRegex.Match(input).Groups[1].Value));
        }

        return DateTime.MinValue;
    }
}
