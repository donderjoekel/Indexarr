using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.IndexedMangas;

public interface IIndexedMangaRepository : IBasicRepository<IndexedManga>
{
    IEnumerable<IndexedManga> GetForIndexer(int indexerId);
    IndexedManga GetByUrl(string url);
}

public class IndexedMangaRepository : BasicRepository<IndexedManga>, IIndexedMangaRepository
{
    public IndexedMangaRepository(IMainDatabase database, IEventAggregator eventAggregator)
        : base(database, eventAggregator)
    {
    }

    public IEnumerable<IndexedManga> GetForIndexer(int indexerId)
    {
        return Query(x => x.IndexerId == indexerId);
    }

    public IndexedManga GetByUrl(string url)
    {
        return Query(x => x.Url == url).SingleOrDefault();
    }
}
