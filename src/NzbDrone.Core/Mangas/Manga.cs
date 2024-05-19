using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Mangas;

public class Manga : ModelBase
{
    public Manga()
    {
        MangaUpdatesTitles = new List<string>();
        AniListTitles = new List<string>();
        MyAnimeListTitles = new List<string>();
    }

    public long? MangaUpdatesId { get; set; }
    public int? MyAnimeListId { get; set; }
    public int? AniListId { get; set; }
    public List<string> MangaUpdatesTitles { get; set; }
    public List<string> AniListTitles { get; set; }
    public List<string> MyAnimeListTitles { get; set; }
}
