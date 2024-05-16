using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.IndexedMangas;

public interface IIndexedMangaService
{
    IndexedManga AddOrUpdate(MangaInfo mangaInfo);
}

public class IndexedMangaService : IIndexedMangaService
{
    private readonly IIndexedMangaRepository _indexedMangaRepository;

    public IndexedMangaService(IIndexedMangaRepository indexedMangaRepository)
    {
        _indexedMangaRepository = indexedMangaRepository;
    }

    public IndexedManga AddOrUpdate(MangaInfo mangaInfo)
    {
        var indexedManga = _indexedMangaRepository.GetByUrl(mangaInfo.Url);

        if (indexedManga == null)
        {
            indexedManga = new IndexedManga
            {
                Url = mangaInfo.Url,
                Title = mangaInfo.Title,
                IndexerId = mangaInfo.IndexerId
            };

            indexedManga = _indexedMangaRepository.Insert(indexedManga);
        }
        else
        {
            indexedManga.Title = mangaInfo.Title;
            indexedManga = _indexedMangaRepository.Update(indexedManga);
        }

        return indexedManga;
    }
}
