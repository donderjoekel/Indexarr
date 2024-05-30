using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NLog;
using NzbDrone.Core.Chapters;
using NzbDrone.Core.Concurrency;
using NzbDrone.Core.Drones;
using NzbDrone.Core.Drones.Events;
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
    IExecute<FullIndexCommand>,
    IExecute<PartialIndexCommand>,
    IHandle<PartialIndexFinishedEvent>
{
    private readonly Logger _logger;
    private readonly IIndexerFactory _indexerFactory;
    private readonly IIndexedMangaService _indexedMangaService;
    private readonly IChapterService _chapterService;
    private readonly IEventAggregator _eventAggregator;
    private readonly IDroneService _droneService;
    private readonly List<Guid> _indexInProgress;

    private SemaphoreSlim _semaphore;

    public IndexingService(Logger logger,
        IIndexerFactory indexerFactory,
        IIndexedMangaService indexedMangaService,
        IChapterService chapterService,
        IEventAggregator eventAggregator,
        IDroneService droneService)
    {
        _logger = logger;
        _indexerFactory = indexerFactory;
        _indexedMangaService = indexedMangaService;
        _chapterService = chapterService;
        _eventAggregator = eventAggregator;
        _droneService = droneService;

        _indexInProgress = new List<Guid>();
    }

    public void Execute(FullIndexCommand message)
    {
        if (!_droneService.IsDirector())
        {
            return;
        }

        if (_droneService.GetDroneCount() == 0)
        {
            _logger.Warn("No drones available to start index");
            return;
        }

        _semaphore = new SemaphoreSlim(_droneService.GetDroneCount());

        var indexers = _indexerFactory.Enabled();

        foreach (var indexer in indexers)
        {
            _semaphore.Wait();
            if (_droneService.DispatchPartialIndex(indexer.Definition.Id))
            {
                _indexInProgress.Add(indexer.Definition.Id);
            }
            else
            {
                _semaphore.Release();
            }
        }

        while (_indexInProgress.Any())
        {
            Thread.Sleep(1000);
        }

        _eventAggregator.PublishEvent(new IndexCompletedEvent());
    }

    public void Execute(PartialIndexCommand message)
    {
        var indexer = _indexerFactory.GetByGuid(message.IndexerId);
        _logger.Info("Starting full index for {Indexer}", indexer.Name);
        var result = indexer.FullIndex().GetAwaiter().GetResult();
        ConcurrentWork.CreateAndRun(1, result.Mangas, info => () => ProcessManga(info));
        _logger.Info("Finished full index for {Indexer}", indexer.Name);
        _droneService.DispatchPartialIndexFinished(message.IndexerId);
    }

    private void ProcessManga(MangaInfo manga)
    {
        IndexedManga indexedManga;

        try
        {
            _logger.Info("Proccessing {Title}", manga.Title);
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

        LogDuplicateChapters(manga);
        ConcurrentWork.CreateAndRun(5,
            manga.Chapters.DistinctBy(x => $"{x.Volume}-{x.Number}"),
            x => () => ProcessChapter(indexedManga, manga, x));
    }

    private void LogDuplicateChapters(MangaInfo manga)
    {
        var groups = manga.Chapters.GroupBy(x => $"{x.Volume}-{x.Number}").ToList();
        foreach (var grouping in groups)
        {
            var chapterInfos = grouping.ToList();
            var count = chapterInfos.Count;
            if (count <= 1)
            {
                continue;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Found multiple chapters with the same number:");
            foreach (var chapterInfo in chapterInfos)
            {
                sb.AppendLine($"{chapterInfo.Volume}-{chapterInfo.Number} : {chapterInfo.Url}");
            }

            _logger.Warn(sb.ToString);
        }
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

    public void Handle(PartialIndexFinishedEvent message)
    {
        var guid = Guid.Parse(message.IndexerId);
        if (_indexInProgress.Contains(guid))
        {
            _indexInProgress.Remove(guid);
            _semaphore.Release();
        }
        else
        {
            _logger.Error("Received an invalid partial index finished event for {IndexerId}", message.IndexerId);
        }
    }
}
