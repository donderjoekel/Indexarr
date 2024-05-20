using System;
using System.Linq;
using NLog;
using NzbDrone.Core.IndexedMangas;
using NzbDrone.Core.Indexing.Events;
using NzbDrone.Core.Mangas;
using NzbDrone.Core.Matching.Commands;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Metadata.Jikan;
using NzbDrone.Core.Metadata.MangaUpdates;

namespace NzbDrone.Core.Matching;

public interface IMatchingService
{
}

public class MatchingService : IMatchingService,
                               IExecute<DirectMatchCommand>,
                               IHandle<FullIndexCompletedEvent>
{
    private readonly IMangaUpdatesService _mangaUpdatesService;
    private readonly IIndexedMangaService _indexedMangaService;
    private readonly Logger _logger;
    private readonly IMangaService _mangaService;
    private readonly IJikanService _jikanService;

    public MatchingService(IMangaUpdatesService mangaUpdatesService,
        IIndexedMangaService indexedMangaService,
        Logger logger,
        IMangaService mangaService,
        IJikanService jikanService)
    {
        _mangaUpdatesService = mangaUpdatesService;
        _indexedMangaService = indexedMangaService;
        _logger = logger;
        _mangaService = mangaService;
        _jikanService = jikanService;
    }

    public void Execute(DirectMatchCommand message)
    {
        _logger.Info("Starting direct match process");
        var indexMangas = _indexedMangaService.GetWithoutLinkedManga().ToList();
        _logger.Info("Attempting to link {Count} mangas", indexMangas.Count);

        foreach (var indexedManga in indexMangas)
        {
            try
            {
                TryLinkIndexedManga(indexedManga);
            }
            catch (Exception e)
            {
                _logger.Error(e, "An error occurred while trying to link {Title}", indexedManga.Title);
                _logger.Info("Ending early due to error");
                return;
            }
        }

        _logger.Info("Finished direct match process");
    }

    private void TryLinkIndexedManga(IndexedManga indexedManga)
    {
        if (_mangaService.TryFindByTitle(indexedManga.Title, out var manga))
        {
            if (indexedManga.MangaId == manga.Id)
            {
                return;
            }

            _indexedMangaService.LinkToManga(indexedManga.Id, manga.Id);
        }
        else
        {
            TryMatch(indexedManga);
        }
    }

    private void TryMatch(IndexedManga indexedManga)
    {
        if (!_mangaUpdatesService.TryMatchTitle(indexedManga.Title, out var mangaUpdatesId))
        {
            _logger.Info("No MangaUpdates match found for '{Title}'", indexedManga.Title);
        }

        if (!_jikanService.TryMatchTitle(indexedManga.Title, out var myAnimeListId))
        {
            _logger.Info("No MyAnimeList match found for '{Title}'", indexedManga.Title);
        }

        if (mangaUpdatesId == null && myAnimeListId == null)
        {
            _logger.Info("No match found for '{Title}'", indexedManga.Title);
            return;
        }

        _logger.Info("Found a match for '{Title}': {MangaUpdatesId}|{myAnimeListId}",
            indexedManga.Title,
            mangaUpdatesId,
            myAnimeListId);

        var manga = _mangaService.CreateWithIds(mangaUpdatesId, myAnimeListId);
        _indexedMangaService.LinkToManga(indexedManga.Id, manga.Id);
    }

    public void Handle(FullIndexCompletedEvent message)
    {
        _logger.Info("Starting direct match process");
        var indexMangas = _indexedMangaService.GetWithoutLinkedManga().ToList();
        _logger.Info("Attempting to link {Count} mangas", indexMangas.Count);

        foreach (var indexedManga in indexMangas)
        {
            try
            {
                TryMatch(indexedManga);
            }
            catch (Exception e)
            {
                _logger.Error(e, "An error occurred while trying to link {Title}", indexedManga.Title);
                _logger.Info("Ending early due to error");
                return;
            }
        }

        _logger.Info("Finished direct match process");
    }
}
