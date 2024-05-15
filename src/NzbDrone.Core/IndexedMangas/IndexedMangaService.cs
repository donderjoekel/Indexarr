namespace NzbDrone.Core.IndexedMangas;

public interface IIndexedMangaService
{
}

public class IndexedMangaService : IIndexedMangaService
{
    private readonly IIndexedMangaRepository _indexedMangaRepository;

    public IndexedMangaService(IIndexedMangaRepository indexedMangaRepository)
    {
        _indexedMangaRepository = indexedMangaRepository;
    }
}
