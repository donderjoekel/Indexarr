using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.IndexedMangas.Events;

public class IndexedMangaTitleUpdatedEvent : IEvent
{
    public IndexedMangaTitleUpdatedEvent(IndexedManga indexedManga)
    {
        IndexedManga = indexedManga;
    }

    public IndexedManga IndexedManga { get; }
}
