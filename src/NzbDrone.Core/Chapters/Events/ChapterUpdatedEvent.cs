using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Chapters.Events;

public class ChapterUpdatedEvent : IEvent
{
    public ChapterUpdatedEvent(Chapter chapter)
    {
        Chapter = chapter;
    }

    public Chapter Chapter { get; }
}
