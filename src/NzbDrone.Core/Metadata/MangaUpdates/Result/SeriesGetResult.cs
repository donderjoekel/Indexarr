using NzbDrone.Core.MetadataSource.MangaUpdates.Resource;
using NzbDrone.Core.MetadataSource.MangaUpdates.Resource.SeriesGet;

namespace NzbDrone.Core.MetadataSource.MangaUpdates.Result;

public class SeriesGetResult
{
    public long SeriesId { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }
    public AssociatedResource[] Associated { get; set; }
    public string Description { get; set; }
    public ImageResource Image { get; set; }
    public string Type { get; set; }
    public string Year { get; set; }
    public decimal BayesianRating { get; set; }
    public int RatingVotes { get; set; }
    public GenreResource[] Genres { get; set; }
    public CategoryResource[] Categories { get; set; }
    public long LatestChapter { get; set; }
    public long ForumId { get; set; }
    public string Status { get; set; }
    public bool Licensed { get; set; }
    public bool Completed { get; set; }
    public AnimeResource Anime { get; set; }
    public RelatedSeriesResource[] RelatedSeries { get; set; }
    public AuthorResource[] Authors { get; set; }
    public PublisherResource[] Publishers { get; set; }
    public PublicationResource[] Publications { get; set; }
    public RecommendationResource[] Recommendations { get; set; }
    public CategoryRecommendationResource[] CategoryRecommendations { get; set; }
    public RankResource Rank { get; set; }
    public TimeResource LastUpdated { get; set; }
}
