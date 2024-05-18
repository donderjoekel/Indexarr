using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Mangas;
using NzbDrone.Core.Mangas.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Metadata.MangaUpdates;

namespace NzbDrone.Core.Metadata;

public interface IMetadataService
{
}

public class MetadataService : IMetadataService,
                               IHandle<MangaCreatedEvent>
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
        var titles = new List<string>();
        titles.AddRange(GetMangaUpdatesTitles(manga));
        titles.AddRange(GetMyAnimeListTitles(manga));
        titles.AddRange(GetAniListTitles(manga));
        _mangaService.UpdateTitles(manga.Id, titles);
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
}
