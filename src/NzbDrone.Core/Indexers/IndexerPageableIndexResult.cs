using System.Collections.Generic;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers;

public class IndexerPageableIndexResult
{
    public IndexerPageableIndexResult()
    {
        Mangas = new List<MangaInfo>();
        Queries = new List<IndexerIndexResult>();
    }

    public IList<MangaInfo> Mangas { get; set; }
    public IList<IndexerIndexResult> Queries { get; set; }
}
