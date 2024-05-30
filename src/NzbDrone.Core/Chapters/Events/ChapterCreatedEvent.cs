using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Chapters.Events;

public class ChapterCreatedEvent : IEvent
{
    public ChapterCreatedEvent(Chapter chapter)
    {
        Chapter = chapter;
    }

    public Chapter Chapter { get; }
}
