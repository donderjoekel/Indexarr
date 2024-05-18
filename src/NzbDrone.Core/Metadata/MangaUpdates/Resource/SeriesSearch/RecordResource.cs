using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.MangaUpdates.Resource.SeriesSearch;

public class RecordResource
{
    public RecordResource()
    {
        Genres = new List<GenreResource>();
    }

    public long SeriesId { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }
    public string Description { get; set; }
    public ImageResource Image { get; set; }
    public string Type { get; set; }
    public string Year { get; set; }
    public decimal BayesianRating { get; set; }
    public int RatingVotes { get; set; }
    public List<GenreResource> Genres { get; set; }
    public decimal LatestChapter { get; set; }
    public RankResource Rank { get; set; }
    public TimeResource LastUpdated { get; set; }
}
