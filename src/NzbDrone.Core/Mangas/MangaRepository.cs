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

    private bool TitleMatches(Manga manga, string title)
    {
        foreach (var mangaTitle in manga.Titles)
        {
            if (title.ReplaceQuotations().EqualsIgnoreCase(mangaTitle.ReplaceQuotations()))
            {
                return true;
            }
        }

        return false;
    }
}
