namespace NzbDrone.Core.MetadataSource.MangaUpdates.Resource.SeriesGet;

public class PublicationResource
{
    public string PublicationName { get; set; }
    public string PublisherName { get; set; }
    public string PublisherId { get; set; } // TODO: The API documentation describes this as a string but I would expect a long
}
