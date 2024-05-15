using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.IndexedMangas;

public interface IIndexedMangaRepository : IBasicRepository<IndexedManga>
{
}

public class IndexedMangaRepository : BasicRepository<IndexedManga>, IIndexedMangaRepository
{
    public IndexedMangaRepository(IDatabase database, IEventAggregator eventAggregator)
        : base(database, eventAggregator)
    {
    }
}
