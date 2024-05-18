using System;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.IndexedMangas;

namespace NzbDrone.Core.Chapters;

public class Chapter : ModelBase
{
    public int IndexedMangaId { get; set; }
    public int Volume { get; set; }
    public decimal Number { get; set; }
    public string Url { get; set; }
    public DateTime Date { get; set; }
    public LazyLoaded<IndexedManga> IndexedManga { get; set; }
}
