using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.IndexedMangas.Events;

public class IndexedMangaLinkedEvent : IEvent
{
    public IndexedMangaLinkedEvent(IndexedManga indexedManga)
    {
        IndexedManga = indexedManga;
    }

    public IndexedManga IndexedManga { get; }
}
