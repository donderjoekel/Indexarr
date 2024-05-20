using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Mangas;

public interface IMangaRepository : IBasicRepository<Manga>
{
    Manga GetByMangaUpdatesId(long mangaUpdatesId);
    Manga GetByMyAnimeListId(int myAnimeListId);
    Manga GetByAniListId(int aniListId);
    Manga GetByTitle(string title);

    IEnumerable<Manga> GetMangasWithoutMangaUpdatesTitles();
    IEnumerable<Manga> GetMangasWithoutAniListTitles();
    IEnumerable<Manga> GetMangasWithoutMyAnimeListTitles();
}

public class MangaRepository : BasicRepository<Manga>, IMangaRepository
{
    public MangaRepository(IMainDatabase database, IEventAggregator eventAggregator)
        : base(database, eventAggregator)
    {
    }

    public Manga GetByMangaUpdatesId(long mangaUpdatesId)
    {
        return Query(x => x.MangaUpdatesId == mangaUpdatesId).SingleOrDefault();
    }

    public Manga GetByMyAnimeListId(int myAnimeListId)
    {
        return Query(x => x.MyAnimeListId == myAnimeListId).SingleOrDefault();
    }

    public Manga GetByAniListId(int aniListId)
    {
        return Query(x => x.AniListId == aniListId).SingleOrDefault();
    }

    public Manga GetByTitle(string title)
    {
        return All().FirstOrDefault(x => TitleMatches(x, title));
    }

    public IEnumerable<Manga> GetMangasWithoutMangaUpdatesTitles()
    {
        return Query(x => x.MangaUpdatesId.HasValue && x.MangaUpdatesTitles.Count > 0);
    }

    public IEnumerable<Manga> GetMangasWithoutAniListTitles()
    {
        return Query(x => x.AniListId.HasValue && x.AniListTitles.Count > 0);
    }

    public IEnumerable<Manga> GetMangasWithoutMyAnimeListTitles()
    {
        return Query(x => x.MyAnimeListId.HasValue && x.MyAnimeListTitles.Count > 0);
    }

    private bool TitleMatches(Manga manga, string title)
    {
        return TitleMatches(title, manga.MangaUpdatesTitles) ||
               TitleMatches(title, manga.MyAnimeListTitles) ||
               TitleMatches(title, manga.AniListTitles);
    }

    private bool TitleMatches(string title, IEnumerable<string> titles)
    {
        foreach (var existingTitle in titles)
        {
            if (title.HtmlDecode().ReplaceQuotations().EqualsIgnoreCase(existingTitle.ReplaceQuotations()))
            {
                return true;
            }
        }

        return false;
    }
}
