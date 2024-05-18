using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Mangas.Events;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Mangas;

public interface IMangaService
{
    Manga GetByMangaUpdatesId(long mangaUpdatesId);
    bool TryFindByTitle(string title, out Manga manga);
    Manga CreateWithMangaUpdatesId(long mangaUpdatesId);
    void UpdateTitles(int id, IEnumerable<string> titles);
}

public class MangaService : IMangaService
{
    private readonly IMangaRepository _mangaRepository;
    private readonly IEventAggregator _eventAggregator;

    public MangaService(IMangaRepository mangaRepository, IEventAggregator eventAggregator)
    {
        _mangaRepository = mangaRepository;
        _eventAggregator = eventAggregator;
    }

    public Manga GetByMangaUpdatesId(long mangaUpdatesId)
    {
        return _mangaRepository.GetByMangaUpdatesId(mangaUpdatesId);
    }

    public bool TryFindByTitle(string title, out Manga manga)
    {
        manga = _mangaRepository.GetByTitle(title);
        return manga != null;
    }

    public Manga CreateWithMangaUpdatesId(long mangaUpdatesId)
    {
        var manga = new Manga
        {
            MangaUpdatesId = mangaUpdatesId
        };

        var createdManga = _mangaRepository.Insert(manga);
        _eventAggregator.PublishEvent(new MangaCreatedEvent(createdManga));
        return createdManga;
    }

    public void UpdateTitles(int id, IEnumerable<string> titles)
    {
        var manga = _mangaRepository.Get(id);
        var changed = false;
        foreach (var title in titles)
        {
            var formattedTitle = title.HtmlDecode().ReplaceQuotations();
            if (!manga.Titles.Contains(formattedTitle))
            {
                manga.Titles.Add(formattedTitle);
                changed = true;
            }
        }

        if (changed)
        {
            _mangaRepository.Update(manga);
            _eventAggregator.PublishEvent(new MangaTitlesUpdatedEvent(manga));
        }
    }
}
