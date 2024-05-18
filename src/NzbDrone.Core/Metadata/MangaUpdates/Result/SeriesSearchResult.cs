using System.Collections.Generic;
using NzbDrone.Core.MetadataSource.MangaUpdates.Resource.SeriesSearch;

namespace NzbDrone.Core.MetadataSource.MangaUpdates.Result;

public class SeriesSearchResult
{
    public int TotalHits { get; set; }
    public int Page { get; set; }
    public int PerPage { get; set; }
    public List<SeriesSearchResultResource> Results { get; set; }
}
