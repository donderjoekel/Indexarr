namespace NzbDrone.Core.MetadataSource.MangaUpdates.Resource.SeriesGet;

public class CategoryRecommendationResource
{
    public string SeriesName { get; set; }
    public long SeriesId { get; set; }
    public int Weight { get; set; }
}
