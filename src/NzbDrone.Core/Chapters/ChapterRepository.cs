using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Chapters;

public interface IChapterRepository : IBasicRepository<Chapter>
{
    IEnumerable<Chapter> GetForIndexedManga(Guid indexedMangaId);
    IEnumerable<Chapter> GetForIndexedManga(Guid indexedMangaId, int volume);
    Chapter GetForIndexedManga(Guid indexedMangaId, int volume, decimal chapterNumber);
}

public class ChapterRepository : BasicRepository<Chapter>, IChapterRepository
{
    public ChapterRepository(IMainDatabase database, IEventAggregator eventAggregator)
        : base(database, eventAggregator)
    {
    }

    public IEnumerable<Chapter> GetForIndexedManga(Guid indexedMangaId)
    {
        return Query(x => x.IndexedMangaId == indexedMangaId);
    }

    public IEnumerable<Chapter> GetForIndexedManga(Guid indexedMangaId, int volume)
    {
        return Query(x => x.IndexedMangaId == indexedMangaId && x.Volume == volume);
    }

    public Chapter GetForIndexedManga(Guid indexedMangaId, int volume, decimal chapterNumber)
    {
        return Query(x => x.IndexedMangaId == indexedMangaId && x.Volume == volume && x.Number == chapterNumber)
            .SingleOrDefault();
    }
}
