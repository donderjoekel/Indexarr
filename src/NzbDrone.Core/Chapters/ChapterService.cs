using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.IndexedMangas;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Chapters;

public interface IChapterService
{
    IEnumerable<Chapter> GetForIndexedManga(Guid indexedMangaId);
    void CreateOrUpdateChapters(IndexedManga indexedManga, MangaInfo mangaInfo);
}

public class ChapterService : IChapterService
{
    private readonly IChapterRepository _chapterRepository;
    private readonly Logger _logger;

    public ChapterService(IChapterRepository chapterRepository, Logger logger)
    {
        _chapterRepository = chapterRepository;
        _logger = logger;
    }

    public IEnumerable<Chapter> GetForIndexedManga(Guid indexedMangaId)
    {
        return _chapterRepository.GetForIndexedManga(indexedMangaId);
    }

    public void CreateOrUpdateChapters(IndexedManga indexedManga, MangaInfo mangaInfo)
    {
        _ = indexedManga ?? throw new ArgumentNullException(nameof(indexedManga));
        _ = mangaInfo ?? throw new ArgumentNullException(nameof(mangaInfo));

        var chapters = GetForIndexedManga(indexedManga.Id).ToList();
        var newChapters = new List<ChapterInfo>();
        var existingChapters = new List<ChapterInfo>();

        foreach (var chapter in mangaInfo.Chapters)
        {
            if (chapters.Any(x => x.Volume == chapter.Volume && x.Number == chapter.Number))
            {
                existingChapters.Add(chapter);
            }
            else
            {
                newChapters.Add(chapter);
            }
        }

        CreateNewChapters(indexedManga.Id, newChapters);
        UpdateExistingChapters(chapters, existingChapters);

        if (newChapters.Count + existingChapters.Count != chapters.Count)
        {
            _logger.Warn("Expected {0} chapters, but found {1} ({2} new, {3} existing)",
                mangaInfo.Chapters.Count,
                chapters.Count,
                newChapters.Count,
                existingChapters.Count);
        }
        else
        {
            _logger.Info(
                "Processed {0}; Created {1}; Updated {2}",
                mangaInfo.Chapters.Count,
                newChapters.Count,
                existingChapters.Count);
        }
    }

    private void CreateNewChapters(Guid indexedMangaId, IEnumerable<ChapterInfo> chapterInfos)
    {
        var chapters = new List<Chapter>();
        foreach (var chapterInfo in chapterInfos)
        {
            chapters.Add(new Chapter()
            {
                IndexedMangaId = indexedMangaId,
                Volume = chapterInfo.Volume,
                Number = chapterInfo.Number,
                Url = chapterInfo.Url,
                Date = chapterInfo.Date
            });
        }

        _chapterRepository.InsertMany(chapters);
    }

    private void UpdateExistingChapters(List<Chapter> chapters, List<ChapterInfo> existingChapters)
    {
        var chaptersToUpdate = new List<Chapter>();

        foreach (var existingChapter in existingChapters)
        {
            var chapter = chapters.FirstOrDefault(
                x => x.Volume == existingChapter.Volume && x.Number == existingChapter.Number);

            if (chapter == null)
            {
                _logger.Error(
                    "Chapter {0}-{1} should exist but doesn't ({2})",
                    existingChapter.Volume,
                    existingChapter.Number,
                    existingChapter.Url);
                continue;
            }

            var hasChanged = false;
            if (chapter.Url != existingChapter.Url)
            {
                chapter.Url = existingChapter.Url;
                hasChanged = true;
            }

            if (chapter.Date != existingChapter.Date)
            {
                chapter.Date = existingChapter.Date;
                hasChanged = true;
            }

            if (hasChanged)
            {
                chaptersToUpdate.Add(chapter);
            }
        }

        _chapterRepository.UpdateMany(chaptersToUpdate);
    }
}
