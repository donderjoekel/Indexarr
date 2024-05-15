using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Chapters;

public interface IChapterRepository : IBasicRepository<Chapter>
{
}

public class ChapterRepository : BasicRepository<Chapter>, IChapterRepository
{
    public ChapterRepository(IDatabase database, IEventAggregator eventAggregator)
        : base(database, eventAggregator)
    {
    }
}
