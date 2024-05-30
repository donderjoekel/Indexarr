namespace NzbDrone.Core.MetadataSource.MangaUpdates.Resource.SeriesGet;

public class RelatedSeriesResource
{
    public long RelationId { get; set; }
    public string RelationType { get; set; }
    public long RelatedSeriesId { get; set; }
    public string RelatedSeriesName { get; set; }
    public long TriggeredByRelationId { get; set; }
}
