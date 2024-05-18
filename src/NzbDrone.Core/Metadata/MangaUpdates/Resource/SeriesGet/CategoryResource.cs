namespace NzbDrone.Core.MetadataSource.MangaUpdates.Resource.SeriesGet;

public class CategoryResource
{
    public long SeriesId { get; set; }
    public string Category { get; set; }
    public int Votes { get; set; }
    public int VotesPlus { get; set; }
    public int VotesMinus { get; set; }
    public long AddedBy { get; set; }
}
