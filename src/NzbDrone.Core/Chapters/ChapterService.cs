using System;
using System.Collections.Generic;
using NzbDrone.Core.Chapters.Events;
using NzbDrone.Core.IndexedMangas;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Chapters;

public interface IChapterService
{
    IEnumerable<Chapter> GetForIndexedManga(Guid indexedMangaId);
    bool Exists(IndexedManga indexedManga, ChapterInfo chapterInfo);
    Chapter Create(IndexedManga indexedManga, ChapterInfo chapterInfo);
    Chapter Update(IndexedManga indexedManga, ChapterInfo chapterInfo);
}

public class ChapterService : IChapterService
{
    private readonly IChapterRepository _chapterRepository;
    private readonly IEventAggregator _eventAggregator;

    public ChapterService(IChapterRepository chapterRepository, IEventAggregator eventAggregator)
    {
        _chapterRepository = chapterRepository;
        _eventAggregator = eventAggregator;
    }

    public IEnumerable<Chapter> GetForIndexedManga(Guid indexedMangaId)
    {
        return _chapterRepository.GetForIndexedManga(indexedMangaId);
    }

    public bool Exists(IndexedManga indexedManga, ChapterInfo chapterInfo)
    {
        _ = indexedManga ?? throw new ArgumentNullException(nameof(indexedManga));
        _ = chapterInfo ?? throw new ArgumentNullException(nameof(chapterInfo));

        return _chapterRepository.GetForIndexedManga(indexedManga.Id, chapterInfo.Volume, chapterInfo.Number) != null;
    }

    public Chapter Create(IndexedManga indexedManga, ChapterInfo chapterInfo)
    {
        _ = indexedManga ?? throw new ArgumentNullException(nameof(indexedManga));
        _ = chapterInfo ?? throw new ArgumentNullException(nameof(chapterInfo));

        var chapter = _chapterRepository.Insert(new Chapter()
        {
            IndexedMangaId = indexedManga.Id,
            Volume = chapterInfo.Volume,
            Number = chapterInfo.Number,
            Url = chapterInfo.Url,
            Date = chapterInfo.Date
        });

        _eventAggregator.PublishEvent(new ChapterCreatedEvent(chapter));
        return chapter;
    }

    public Chapter Update(IndexedManga indexedManga, ChapterInfo chapterInfo)
    {
        _ = indexedManga ?? throw new ArgumentNullException(nameof(indexedManga));
        _ = chapterInfo ?? throw new ArgumentNullException(nameof(chapterInfo));

        var chapter = _chapterRepository.GetForIndexedManga(indexedManga.Id, chapterInfo.Volume, chapterInfo.Number);
        var changed = false;
        if (chapter.Url != chapterInfo.Url)
        {
            chapter.Url = chapterInfo.Url;
            changed = true;
        }

        if (chapter.Date != chapterInfo.Date)
        {
            chapter.Date = chapterInfo.Date;
            changed = true;
        }

        if (changed)
        {
            chapter = _chapterRepository.Update(chapter);
            _eventAggregator.PublishEvent(new ChapterUpdatedEvent(chapter));
        }

        return chapter;
    }
}
