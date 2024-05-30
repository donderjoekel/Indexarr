using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Drones.Events;

public class PartialIndexFinishedEvent : IEvent
{
    public string IndexerId { get; set; }
}
