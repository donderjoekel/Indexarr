using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Mangas.Events;

public class MangaCreatedEvent : IEvent
{
    public MangaCreatedEvent(Manga manga)
    {
        Manga = manga;
    }

    public Manga Manga { get; }
}
