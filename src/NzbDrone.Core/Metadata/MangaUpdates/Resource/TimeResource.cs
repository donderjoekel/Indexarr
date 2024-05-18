using Newtonsoft.Json;

namespace NzbDrone.Core.MetadataSource.MangaUpdates.Resource;

public class TimeResource
{
    public long Timestamp { get; set; }

    // ReSharper disable once InconsistentNaming - RFC3339 is an abbreviation
    [JsonProperty("as_rfc3339")]
    public string AsRFC3339 { get; set; }
    public string AsString { get; set; }
}
