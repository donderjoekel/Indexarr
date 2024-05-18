using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.MangaUpdates.Resource.ReleaseSearch;

public class RecordResource
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Volume { get; set; }
    public string Chapter { get; set; }
    public List<GroupResource> Groups { get; set; }
    public string ReleaseDate { get; set; }
}
