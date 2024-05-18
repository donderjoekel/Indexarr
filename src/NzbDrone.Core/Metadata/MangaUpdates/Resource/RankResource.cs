namespace NzbDrone.Core.MetadataSource.MangaUpdates.Resource;

public class RankResource
{
    public PositionResource Position { get; set; }
    public PositionResource OldPosition { get; set; }
    public ListsResource Lists { get; set; }
}
