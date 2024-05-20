using NLog;
using NzbDrone.Core.Mangas;
using NzbDrone.Core.Matching.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Metadata.Jikan;
using NzbDrone.Core.Metadata.MangaUpdates;

namespace NzbDrone.Core.Metadata;

public interface IMetadataService
{
}

public class MetadataService : IMetadataService,
                               IHandle<MatchingCompletedEvent>
{
    private readonly IMangaUpdatesService _mangaUpdatesService;
    private readonly IMangaService _mangaService;
    private readonly Logger _logger;
    private readonly IJikanService _jikanService;

    public MetadataService(IMangaUpdatesService mangaUpdatesService,
        IMangaService mangaService,
        Logger logger,
        IJikanService jikanService)
    {
        _mangaUpdatesService = mangaUpdatesService;
        _mangaService = mangaService;
        _logger = logger;
        _jikanService = jikanService;
    }

    public void Handle(MatchingCompletedEvent message)
    {
        _logger.Info("Starting refresh of all titles");

        var mangas = _mangaService.GetMangasWithoutMangaUpdatesTitles();
        foreach (var manga in mangas)
        {
            _mangaService.UpdateMangaUpdatesTitles(manga.Id,
                _mangaUpdatesService.GetTitles(manga.MangaUpdatesId!.Value));
        }

        mangas = _mangaService.GetMangasWithoutMyAnimeListTitles();
        foreach (var manga in mangas)
        {
            _mangaService.UpdateMyAnimeListTitles(manga.Id,
                _jikanService.GetTitles(manga.MyAnimeListId!.Value));
        }

        // TODO: Implement when AniList is implemented
        /*mangas = _mangaService.GetMangasWithoutAniListTitles();
        foreach (var manga in mangas)
        {

            _mangaService.UpdateAniListTitles(
                manga.Id,
                _mangaUpdatesService.GetTitles(manga.MangaUpdatesId!.Value));
        }*/

        _logger.Info("Finished refresh of all titles");
    }
}
