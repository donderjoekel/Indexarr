using System;
using NLog;
using NzbDrone.Core.Chapters;
using NzbDrone.Core.Concurrency;
using NzbDrone.Core.IndexedMangas;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexing.Commands;
using NzbDrone.Core.Indexing.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;

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
    private readonly IEventAggregator _eventAggregator;

    public IndexingService(Logger logger,
        IIndexerFactory indexerFactory,
        IIndexedMangaService indexedMangaService,
        IChapterService chapterService,
        IEventAggregator eventAggregator)
    {
        _logger = logger;
        _indexerFactory = indexerFactory;
        _indexedMangaService = indexedMangaService;
        _chapterService = chapterService;
        _eventAggregator = eventAggregator;
    }

    public void Execute(FullIndexCommand message)
    {
        var indexers = _indexerFactory.Enabled();

        foreach (var indexer in indexers)
        {
            _logger.Info("Starting full index for {Indexer}", indexer.Name);
            var result = indexer.FullIndex().GetAwaiter().GetResult();
            ConcurrentWork.CreateAndRun(5, result.Mangas, info => () => ProcessManga(info));
            _logger.Info("Finished full index for {Indexer}", indexer.Name);
        }

        _eventAggregator.PublishEvent(new IndexCompletedEvent());
    }

    private void ProcessManga(MangaInfo manga)
    {
        IndexedManga indexedManga;

        try
        {
            _logger.Info("Prccessing {Title}", manga.Title);
            if (_indexedMangaService.Exists(manga))
            {
                _logger.Debug("Updating {Title}", manga.Title);
                indexedManga = _indexedMangaService.Update(manga);
            }
            else
            {
                _logger.Debug("Creating {Title}", manga.Title);
                indexedManga = _indexedMangaService.Create(manga);
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error processing manga {Title}", manga.Title);
            return;
        }

        ConcurrentWork.CreateAndRun(5, manga.Chapters, x => () => ProcessChapter(indexedManga, manga, x));
    }

    private void ProcessChapter(IndexedManga indexedManga, MangaInfo manga, ChapterInfo chapter)
    {
        try
        {
            if (_chapterService.Exists(indexedManga, chapter))
            {
                _logger.Debug("Updating {Title} - {Chapter}", manga.Title, chapter.Number);
                _chapterService.Update(indexedManga, chapter);
            }
            else
            {
                _logger.Debug("Creating {Title} - {Chapter}", manga.Title, chapter.Number);
                _chapterService.Create(indexedManga, chapter);
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error processing chapter {Title} - {Chapter}", manga.Title, chapter.Number);
        }
    }
}
