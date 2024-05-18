using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Chapters;

public interface IChapterRepository : IBasicRepository<Chapter>
{
    IEnumerable<Chapter> GetForIndexedManga(int indexedMangaId);
    IEnumerable<Chapter> GetForIndexedManga(int indexedMangaId, int volume);
    Chapter GetForIndexedManga(int indexedMangaId, int volume, decimal chapterNumber);
}

public class ChapterRepository : BasicRepository<Chapter>, IChapterRepository
{
    public ChapterRepository(IMainDatabase database, IEventAggregator eventAggregator)
        : base(database, eventAggregator)
    {
    }

    public IEnumerable<Chapter> GetForIndexedManga(int indexedMangaId)
    {
        return Query(x => x.IndexedMangaId == indexedMangaId);
    }

    public IEnumerable<Chapter> GetForIndexedManga(int indexedMangaId, int volume)
    {
        return Query(x => x.IndexedMangaId == indexedMangaId && x.Volume == volume);
    }

    public Chapter GetForIndexedManga(int indexedMangaId, int volume, decimal chapterNumber)
    {
        return Query(x => x.IndexedMangaId == indexedMangaId && x.Volume == volume && x.Number == chapterNumber)
            .SingleOrDefault();
    }
}
