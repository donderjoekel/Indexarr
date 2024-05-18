using System;
using NLog;
using NzbDrone.Core.Chapters;
using NzbDrone.Core.Concurrency;
using NzbDrone.Core.IndexedMangas;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexing.Commands;
using NzbDrone.Core.Messaging.Commands;
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

    public IndexingService(Logger logger,
        IIndexerFactory indexerFactory,
        IIndexedMangaService indexedMangaService,
        IChapterService chapterService)
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
            _logger.Info("Starting full index for {Indexer}", indexer.Name);
            var result = indexer.FullIndex().GetAwaiter().GetResult();

            ConcurrentWork.CreateAndRun(5, result.Mangas, info => () => ProcessManga(info));

            /*var mangaSemaphore = new SemaphoreSlim(5);
            var mangaTasks = new List<Task>();

            foreach (var manga in result.Mangas)
            {
                mangaTasks.Add(Task.Run(async () =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    await mangaSemaphore.WaitAsync();

                    try
                    {
                        ProcessManga(manga);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Error processing manga {Title}", manga.Title);
                    }
                    finally
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        mangaSemaphore.Release();
                    }
                }));
            }

            Task.WhenAll(mangaTasks).GetAwaiter().GetResult();
            mangaSemaphore.Dispose();*/

            /*foreach (var mangaInfo in result.Mangas)
            {
                IndexedManga indexedManga;

                if (_indexedMangaService.Exists(mangaInfo))
                {
                    _logger.Info("Updating {Title}", mangaInfo.Title);
                    indexedManga = _indexedMangaService.Update(mangaInfo);
                }
                else
                {
                    _logger.Info("Creating {Title}", mangaInfo.Title);
                    indexedManga = _indexedMangaService.Create(mangaInfo);
                }

                var tasks = new List<Task>();

                foreach (var chapterInfo in mangaInfo.Chapters)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        if (_chapterService.Exists(indexedManga, chapterInfo))
                        {
                            _logger.Info("Updating {Title} - {Chapter}", mangaInfo.Title, chapterInfo.Number);
                            _chapterService.Update(indexedManga, chapterInfo);
                        }
                        else
                        {
                            _logger.Info("Creating {Title} - {Chapter}", mangaInfo.Title, chapterInfo.Number);
                            _chapterService.Create(indexedManga, chapterInfo);
                        }
                    }));
                }

                Task.WhenAll(tasks).GetAwaiter().GetResult();
            }*/

            _logger.Info("Finished full index for {Indexer}", indexer.Name);
        }
    }

    private void ProcessManga(MangaInfo manga)
    {
        IndexedManga indexedManga;

        try
        {
            if (_indexedMangaService.Exists(manga))
            {
                _logger.Info("Updating {Title}", manga.Title);
                indexedManga = _indexedMangaService.Update(manga);
            }
            else
            {
                _logger.Info("Creating {Title}", manga.Title);
                indexedManga = _indexedMangaService.Create(manga);
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error processing manga {Title}", manga.Title);
            return;
        }

        ConcurrentWork.CreateAndRun(5, manga.Chapters, x => () => ProcessChapter(indexedManga, manga, x));

        /*var chapterSemaphore = new SemaphoreSlim(5);
        var chapterTasks = new List<Task>();

        foreach (var chapterInfo in manga.Chapters)
        {
            chapterTasks.Add(Task.Run(async () =>
            {
                // ReSharper disable once AccessToDisposedClosure
                await chapterSemaphore.WaitAsync();

                try
                {
                    ProcessChapter(indexedManga, manga, chapterInfo);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Error processing chapter {Title} - {Chapter}", manga.Title, chapterInfo.Number);
                }
                finally
                {
                    // ReSharper disable once AccessToDisposedClosure
                    chapterSemaphore.Release();
                }
            }));
        }

        Task.WhenAll(chapterTasks).GetAwaiter().GetResult();
        chapterSemaphore.Dispose();*/
    }

    private void ProcessChapter(IndexedManga indexedManga, MangaInfo manga, ChapterInfo chapter)
    {
        try
        {
            if (_chapterService.Exists(indexedManga, chapter))
            {
                _logger.Info("Updating {Title} - {Chapter}", manga.Title, chapter.Number);
                _chapterService.Update(indexedManga, chapter);
            }
            else
            {
                _logger.Info("Creating {Title} - {Chapter}", manga.Title, chapter.Number);
                _chapterService.Create(indexedManga, chapter);
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error processing chapter {Title} - {Chapter}", manga.Title, chapter.Number);
        }
    }
}
