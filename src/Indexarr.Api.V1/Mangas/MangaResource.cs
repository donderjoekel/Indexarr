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
    public static MangaResource ToResource(this Manga model)
    {
        return new MangaResource()
        {
            Id = model.Id,
            MangaUpdatesId = model.MangaUpdatesId,
            MyAnimeListId = model.MyAnimeListId,
            AniListId = model.AniListId,
            Titles = model.MangaUpdatesTitles.Concat(model.AniListTitles).Concat(model.MyAnimeListTitles).ToList()
        };
    }
}
