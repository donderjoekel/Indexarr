using Indexarr.Core.Purging.Commands;
using NzbDrone.Core.Chapters;
using NzbDrone.Core.IndexedMangas;
using NzbDrone.Core.Mangas;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Purging;

public interface IPurgeService
{
    void Purge();
}

public class PurgeService : IPurgeService, IExecute<PurgeCommand>
{
    private readonly IMangaRepository _mangaRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IIndexedMangaRepository _indexedMangaRepository;

    public PurgeService(IMangaRepository mangaRepository,
        IChapterRepository chapterRepository,
        IIndexedMangaRepository indexedMangaRepository)
    {
        _mangaRepository = mangaRepository;
        _chapterRepository = chapterRepository;
        _indexedMangaRepository = indexedMangaRepository;
    }

    public void Purge()
    {
        _mangaRepository.Purge();
        _chapterRepository.Purge();
        _indexedMangaRepository.Purge();
    }

    public void Execute(PurgeCommand message)
    {
        Purge();
    }
}
