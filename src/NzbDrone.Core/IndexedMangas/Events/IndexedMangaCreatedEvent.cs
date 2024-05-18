using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.IndexedMangas.Events;

public class IndexedMangaCreatedEvent : IEvent
{
    public IndexedMangaCreatedEvent(IndexedManga indexedManga)
    {
        IndexedManga = indexedManga;
    }

    public IndexedManga IndexedManga { get; }
}
