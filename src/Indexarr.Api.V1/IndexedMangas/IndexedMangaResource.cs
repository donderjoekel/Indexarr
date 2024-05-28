using NzbDrone.Core.IndexedMangas;
using Prowlarr.Http.REST;

namespace Prowlarr.Api.V1.IndexedMangas;

public class IndexedMangaResource : RestResource
{
    public int? MangaId { get; set; }
    public int IndexerId { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }
}

public static class IndexedMangaResourceMapper
{
    public static IndexedMangaResource ToResource(this IndexedManga model)
    {
        return new IndexedMangaResource()
        {
            Id = model.Id,
            MangaId = model.MangaId,
            IndexerId = model.IndexerId,
            Title = model.Title,
            Url = model.Url
        };
    }
}
