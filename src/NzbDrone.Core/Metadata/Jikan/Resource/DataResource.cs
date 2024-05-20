namespace Indexarr.Core.Metadata.Jikan.Resource;

public class DataResource
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string TitleEnglish { get; set; }
    public string TitleJapanese { get; set; }
    public TitleResource[] Titles { get; set; }
}
