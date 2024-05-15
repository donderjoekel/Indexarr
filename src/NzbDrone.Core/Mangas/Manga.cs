using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Mangas;

public class Manga : ModelBase
{
    public Manga()
    {
        Titles = new List<string>();
    }

    public int MangaUpdatesId { get; set; }
    public int MyAnimeListId { get; set; }
    public int AniListId { get; set; }
    public List<string> Titles { get; set; }
}
