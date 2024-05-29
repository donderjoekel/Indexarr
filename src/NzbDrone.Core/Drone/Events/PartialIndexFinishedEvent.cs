using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Drone.Events;

public class PartialIndexFinishedEvent : IEvent
{
    public string IndexerId { get; set; }
}
