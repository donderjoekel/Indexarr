using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Mangas;
using Prowlarr.Http.REST;

namespace Prowlarr.Api.V1.Mangas;

public class MangaResource : RestResource
{
    public long? MangaUpdatesId { get; set; }
    public int? MyAnimeListId { get; set; }
    public int? AniListId { get; set; }
    public List<string> Titles { get; set; }
}

public static class MangaResourceMapper
{
    public static MangaResource ToResource(this Manga manga)
    {
        return new MangaResource()
        {
            Id = manga.Id,
            MangaUpdatesId = manga.MangaUpdatesId,
            MyAnimeListId = manga.MyAnimeListId,
            AniListId = manga.AniListId,
            Titles = manga.MangaUpdatesTitles.Concat(manga.AniListTitles).Concat(manga.MyAnimeListTitles).ToList()
        };
    }
}
