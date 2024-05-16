using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Chapters;

public interface IChapterRepository : IBasicRepository<Chapter>
{
    Chapter GetForIndexerByChapterNumber(int indexedMangaId, decimal chapterNumber);
}

public class ChapterRepository : BasicRepository<Chapter>, IChapterRepository
{
    public ChapterRepository(IMainDatabase database, IEventAggregator eventAggregator)
        : base(database, eventAggregator)
    {
    }

    public Chapter GetForIndexerByChapterNumber(int indexedMangaId, decimal chapterNumber)
    {
        return Query(x => x.IndexedMangaId == indexedMangaId && x.Number == chapterNumber).SingleOrDefault();
    }
}
