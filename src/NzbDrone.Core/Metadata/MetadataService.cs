using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Indexing.Events;
using NzbDrone.Core.Mangas;
using NzbDrone.Core.Mangas.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Metadata.MangaUpdates;

namespace NzbDrone.Core.Metadata;

public interface IMetadataService
{
}

public class MetadataService : IMetadataService,
                               IHandle<FullIndexCompletedEvent>
{
    private readonly IMangaUpdatesService _mangaUpdatesService;
    private readonly IMangaService _mangaService;
    private readonly Logger _logger;

    public MetadataService(IMangaUpdatesService mangaUpdatesService, IMangaService mangaService, Logger logger)
    {
        _mangaUpdatesService = mangaUpdatesService;
        _mangaService = mangaService;
        _logger = logger;
    }

    public void Handle(MangaCreatedEvent message)
    {
        /*_logger.Info("Starting refresh of titles for {Id}:{Titles}",
            message.Manga.Id,
            string.Join(',', message.Manga.Titles));
        RefreshTitles(message.Manga);
        _logger.Info("Finished refresh of titles for {Id}:{Titles}",
            message.Manga.Id,
            string.Join(',', message.Manga.Titles));*/
    }

    private void RefreshTitles(Manga manga)
    {
        _mangaService.UpdateMangaUpdatesTitles(manga.Id, GetMangaUpdatesTitles(manga));
        _mangaService.UpdateAniListTitles(manga.Id, GetAniListTitles(manga));
        _mangaService.UpdateMyAnimeListTitles(manga.Id, GetMyAnimeListTitles(manga));
    }

    private IEnumerable<string> GetMangaUpdatesTitles(Manga manga)
    {
        return manga.MangaUpdatesId.HasValue
            ? _mangaUpdatesService.GetTitles(manga.MangaUpdatesId.Value)
            : Enumerable.Empty<string>();
    }

    private IEnumerable<string> GetMyAnimeListTitles(Manga manga)
    {
        return Enumerable.Empty<string>();
    }

    private IEnumerable<string> GetAniListTitles(Manga manga)
    {
        return Enumerable.Empty<string>();
    }

    public void Handle(FullIndexCompletedEvent message)
    {
        _logger.Info("Starting refresh of all titles");

        var mangas = _mangaService.GetMangasWithoutMangaUpdatesTitles();
        foreach (var manga in mangas)
        {
            _mangaService.UpdateMangaUpdatesTitles(
                manga.Id,
                _mangaUpdatesService.GetTitles(manga.MangaUpdatesId!.Value));
        }

        mangas = _mangaService.GetMangasWithoutAniListTitles();
        foreach (var manga in mangas)
        {
            // TODO: Implement when AniList is implemented
            /*_mangaService.UpdateAniListTitles(
                manga.Id,
                _mangaUpdatesService.GetTitles(manga.MangaUpdatesId!.Value));*/
        }

        mangas = _mangaService.GetMangasWithoutMyAnimeListTitles();
        foreach (var manga in mangas)
        {
            // TODO: Implement when MyAnimeList is implemented
            /*_mangaService.UpdateMyAnimeListTitles(
                manga.Id,
                _mangaUpdatesService.GetTitles(manga.MangaUpdatesId!.Value));*/
        }

        _logger.Info("Finished refresh of all titles");
    }
}
