using NzbDrone.Core.Datastore;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Mangas;

namespace NzbDrone.Core.IndexedMangas;

public class IndexedManga : ModelBase
{
    public int? MangaId { get; set; }
    public int IndexerId { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }

    public LazyLoaded<Manga> Manga { get; set; }
    public LazyLoaded<IndexerDefinition> Indexer { get; set; }
}
