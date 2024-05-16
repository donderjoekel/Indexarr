using NLog;
using NzbDrone.Core.Chapters;
using NzbDrone.Core.IndexedMangas;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexing.Commands;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Indexing;

public interface IIndexingService
{
}

public class IndexingService : IIndexingService,
                               IExecute<FullIndexCommand>
{
    private readonly Logger _logger;
    private readonly IIndexerFactory _indexerFactory;
    private readonly IIndexedMangaService _indexedMangaService;
    private readonly IChapterService _chapterService;

    public IndexingService(Logger logger, IIndexerFactory indexerFactory, IIndexedMangaService indexedMangaService, IChapterService chapterService)
    {
        _logger = logger;
        _indexerFactory = indexerFactory;
        _indexedMangaService = indexedMangaService;
        _chapterService = chapterService;
    }

    public void Execute(FullIndexCommand message)
    {
        var indexers = _indexerFactory.Enabled();

        foreach (var indexer in indexers)
        {
            _logger.Info("Full index for {Indexer}", indexer.Name);
            var result = indexer.FullIndex().GetAwaiter().GetResult();

            foreach (var mangaInfo in result.Mangas)
            {
                _logger.Trace("Upserting {Title}", mangaInfo.Title);
                var indexedManga = _indexedMangaService.AddOrUpdate(mangaInfo);

                foreach (var chapterInfo in mangaInfo.Chapters)
                {
                    _logger.Trace("Upserting {Title} - {Chapter}", mangaInfo.Title, chapterInfo.Number);
                    _chapterService.AddOrUpdate(indexedManga, chapterInfo);
                }
            }
        }
    }
}
