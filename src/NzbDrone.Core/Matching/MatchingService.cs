using System;
using System.Linq;
using NLog;
using NzbDrone.Core.IndexedMangas;
using NzbDrone.Core.Indexing.Events;
using NzbDrone.Core.Mangas;
using NzbDrone.Core.Matching.Commands;
using NzbDrone.Core.Matching.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Metadata.Jikan;
using NzbDrone.Core.Metadata.MangaUpdates;

namespace NzbDrone.Core.Matching;

public interface IMatchingService
{
}

public class MatchingService : IMatchingService,
                               IExecute<MatchMangasCommand>,
                               IHandle<IndexCompletedEvent>
{
    private readonly IMangaUpdatesService _mangaUpdatesService;
    private readonly IIndexedMangaService _indexedMangaService;
    private readonly Logger _logger;
    private readonly IMangaService _mangaService;
    private readonly IJikanService _jikanService;
    private readonly IEventAggregator _eventAggregator;

    public MatchingService(IMangaUpdatesService mangaUpdatesService,
        IIndexedMangaService indexedMangaService,
        Logger logger,
        IMangaService mangaService,
        IJikanService jikanService,
        IEventAggregator eventAggregator)
    {
        _mangaUpdatesService = mangaUpdatesService;
        _indexedMangaService = indexedMangaService;
        _logger = logger;
        _mangaService = mangaService;
        _jikanService = jikanService;
        _eventAggregator = eventAggregator;
    }

    public void Execute(MatchMangasCommand message)
    {
        MatchMangas();
    }

    public void Handle(IndexCompletedEvent message)
    {
        MatchMangas();
    }

    private void MatchMangas()
    {
        _logger.Info("Starting match process");
        var groups = _indexedMangaService.GetWithoutLinkedManga()
            .OrderBy(x => x.Title)
            .GroupBy(x => x.Title)
            .ToList();

        _logger.Info("Attempting to link {Count} mangas", groups.Count);

        for (var i = 0; i < groups.Count; i++)
        {
            var group = groups[i];
            try
            {
                var first = group.First();

                _logger.Info(
                    "Attempting to match {Title} ({Index}/{Total})",
                    first.Title,
                    i + 1,
                    groups.Count);

                if (!TryLinkIndexedManga(first))
                {
                    continue;
                }

                foreach (var indexedManga in group.Skip(1))
                {
                    TryLinkIndexedManga(indexedManga);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "An error occurred while trying to link manga");
                _logger.Info("Ending early due to error");
                return;
            }
        }

        _logger.Info("Finished match process");
        _eventAggregator.PublishEvent(new MatchingCompletedEvent());
    }

    private bool TryLinkIndexedManga(IndexedManga indexedManga)
    {
        if (_mangaService.TryFindByTitle(indexedManga.Title, out var manga))
        {
            if (indexedManga.MangaId == manga.Id)
            {
                return true;
            }

            _indexedMangaService.LinkToManga(indexedManga.Id, manga.Id);
            return true;
        }

        var linkedIndexedManga = _indexedMangaService.FindByTitle(indexedManga.Title)
            .FirstOrDefault(x => x.MangaId != null);

        if (linkedIndexedManga != null)
        {
            _logger.Info("Found existing linked manga for '{Title}'", indexedManga.Title);
            _indexedMangaService.LinkToManga(indexedManga.Id, linkedIndexedManga.MangaId!.Value);
            return true;
        }

        return TryMatch(indexedManga);
    }

    private bool TryMatch(IndexedManga indexedManga)
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
            return false;
        }

        _logger.Info("Found a match for '{Title}': {MangaUpdatesId}|{myAnimeListId}",
            indexedManga.Title,
            mangaUpdatesId,
            myAnimeListId);

        if (!_mangaService.TryFindByIds(mangaUpdatesId, myAnimeListId, out var manga))
        {
            manga = _mangaService.CreateWithIds(mangaUpdatesId, myAnimeListId);
        }

        _indexedMangaService.LinkToManga(indexedManga.Id, manga.Id);
        return true;
    }
}
