using System.Collections.Generic;
using NzbDrone.Common.Http;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers;

public class IndexerIndexResult
{
    public IndexerIndexResult()
    {
        Mangas = new List<MangaInfo>();
    }

    public IList<MangaInfo> Mangas { get; set; }
    public HttpResponse Response { get; set; }
    public bool Cached { get; set; }
}
