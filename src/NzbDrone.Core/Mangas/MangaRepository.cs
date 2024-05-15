using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Mangas;

public interface IMangaRepository : IBasicRepository<Manga>
{
}

public class MangaRepository : BasicRepository<Manga>, IMangaRepository
{
    public MangaRepository(IDatabase database, IEventAggregator eventAggregator)
        : base(database, eventAggregator)
    {
    }
}
