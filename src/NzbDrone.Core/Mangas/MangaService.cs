using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Mangas.Events;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Mangas;

public interface IMangaService
{
    Manga Find(int id);
    IEnumerable<Manga> All();

    Manga GetByMangaUpdatesId(long mangaUpdatesId);
    bool TryFindByTitle(string title, out Manga manga);
    Manga CreateWithIds(long? mangaUpdatesId = null, int? myAnimeListId = null);

    IEnumerable<Manga> GetMangasWithoutMangaUpdatesTitles();
    IEnumerable<Manga> GetMangasWithoutAniListTitles();
    IEnumerable<Manga> GetMangasWithoutMyAnimeListTitles();

    void UpdateMangaUpdatesTitles(int id, IEnumerable<string> titles);
    void UpdateAniListTitles(int id, IEnumerable<string> titles);
    void UpdateMyAnimeListTitles(int id, IEnumerable<string> titles);
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

    public Manga Find(int id)
    {
        return _mangaRepository.Find(id);
    }

    public IEnumerable<Manga> All()
    {
        return _mangaRepository.All();
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

    public Manga CreateWithIds(long? mangaUpdatesId = null, int? myAnimeListId = null)
    {
        var manga = new Manga()
        {
            MangaUpdatesId = mangaUpdatesId,
            MyAnimeListId = myAnimeListId
        };

        var createdManga = _mangaRepository.Insert(manga);
        _eventAggregator.PublishEvent(new MangaCreatedEvent(createdManga));
        return createdManga;
    }

    public IEnumerable<Manga> GetMangasWithoutMangaUpdatesTitles()
    {
        return _mangaRepository.GetMangasWithoutMangaUpdatesTitles();
    }

    public IEnumerable<Manga> GetMangasWithoutAniListTitles()
    {
        return _mangaRepository.GetMangasWithoutAniListTitles();
    }

    public IEnumerable<Manga> GetMangasWithoutMyAnimeListTitles()
    {
        return _mangaRepository.GetMangasWithoutMyAnimeListTitles();
    }

    public void UpdateMangaUpdatesTitles(int id, IEnumerable<string> titles)
    {
        var manga = _mangaRepository.Get(id);
        var changed = false;
        foreach (var title in titles)
        {
            var formattedTitle = title.HtmlDecode().ReplaceQuotations();
            if (!manga.MangaUpdatesTitles.Contains(formattedTitle))
            {
                manga.MangaUpdatesTitles.Add(formattedTitle);
                changed = true;
            }
        }

        if (changed)
        {
            _mangaRepository.Update(manga);
            _eventAggregator.PublishEvent(new MangaTitlesUpdatedEvent(manga));
        }
    }

    public void UpdateAniListTitles(int id, IEnumerable<string> titles)
    {
        var manga = _mangaRepository.Get(id);
        var changed = false;
        foreach (var title in titles)
        {
            var formattedTitle = title.HtmlDecode().ReplaceQuotations();
            if (!manga.AniListTitles.Contains(formattedTitle))
            {
                manga.AniListTitles.Add(formattedTitle);
                changed = true;
            }
        }

        if (changed)
        {
            _mangaRepository.Update(manga);
            _eventAggregator.PublishEvent(new MangaTitlesUpdatedEvent(manga));
        }
    }

    public void UpdateMyAnimeListTitles(int id, IEnumerable<string> titles)
    {
        var manga = _mangaRepository.Get(id);
        var changed = false;
        foreach (var title in titles)
        {
            var formattedTitle = title.HtmlDecode().ReplaceQuotations();
            if (!manga.MyAnimeListTitles.Contains(formattedTitle))
            {
                manga.MyAnimeListTitles.Add(formattedTitle);
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
