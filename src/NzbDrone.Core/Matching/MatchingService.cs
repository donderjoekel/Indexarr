using System;
using NLog;
using NzbDrone.Core.IndexedMangas;
using NzbDrone.Core.Mangas;
using NzbDrone.Core.Matching.Commands;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Metadata.MangaUpdates;

namespace NzbDrone.Core.Matching;

public interface IMatchingService
{
}

public class MatchingService : IMatchingService,
                               IExecute<DirectMatchCommand>
{
    private readonly IMangaUpdatesService _mangaUpdatesService;
    private readonly IIndexedMangaService _indexedMangaService;
    private readonly Logger _logger;
    private readonly IMangaService _mangaService;

    public MatchingService(IMangaUpdatesService mangaUpdatesService, IIndexedMangaService indexedMangaService, Logger logger, IMangaService mangaService)
    {
        _mangaUpdatesService = mangaUpdatesService;
        _indexedMangaService = indexedMangaService;
        _logger = logger;
        _mangaService = mangaService;
    }

    public void Execute(DirectMatchCommand message)
    {
        var indexMangas = _indexedMangaService.GetWithoutLinkedManga();

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
            TryDirectMatchToMangaUpdates(indexedManga);
        }
    }

    private void TryDirectMatchToMangaUpdates(IndexedManga indexedManga)
    {
        if (!_mangaUpdatesService.TryDirectMatchTitleToId(indexedManga.Title, out var mangaUpdatesId))
        {
            _logger.Info("Failed to find direct match for {Title}", indexedManga.Title);
            return;
        }

        _logger.Info("Found a direct match for {Title}: {Id}", indexedManga.Title, mangaUpdatesId);
        var manga = _mangaService.CreateWithMangaUpdatesId(mangaUpdatesId);
        _indexedMangaService.LinkToManga(indexedManga.Id, manga.Id);
    }
}
