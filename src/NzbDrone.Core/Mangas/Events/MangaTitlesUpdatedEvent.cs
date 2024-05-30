using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Mangas.Events;

public class MangaTitlesUpdatedEvent : IEvent
{
    public MangaTitlesUpdatedEvent(Manga manga)
    {
        Manga = manga;
    }

    public Manga Manga { get; }
}
