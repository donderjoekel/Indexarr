using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.IndexedMangas;

public interface IIndexedMangaRepository : IBasicRepository<IndexedManga>
{
    IEnumerable<IndexedManga> GetByMangaId(Guid mangaId);
    IEnumerable<IndexedManga> GetForIndexer(Guid indexerId);
    IndexedManga GetByUrl(string url);
    IEnumerable<IndexedManga> GetWithoutLinkedManga();
    IEnumerable<IndexedManga> GetWithTitle(string title);
}

public class IndexedMangaRepository : BasicRepository<IndexedManga>, IIndexedMangaRepository
{
    public IndexedMangaRepository(IMainDatabase database, IEventAggregator eventAggregator)
        : base(database, eventAggregator)
    {
    }

    public IEnumerable<IndexedManga> GetByMangaId(Guid mangaId)
    {
        return Query(x => x.MangaId == mangaId);
    }

    public IEnumerable<IndexedManga> GetForIndexer(Guid indexerId)
    {
        return Query(x => x.IndexerId == indexerId);
    }

    public IndexedManga GetByUrl(string url)
    {
        return Query(x => x.Url == url).SingleOrDefault();
    }

    public IEnumerable<IndexedManga> GetWithoutLinkedManga()
    {
        return Query(x => x.MangaId == null);
    }

    public IEnumerable<IndexedManga> GetWithTitle(string title)
    {
        return Query(x => x.Title == title);
    }
}
