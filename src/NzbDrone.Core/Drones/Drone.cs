using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Drones;

public class Drone : ModelBase
{
    public string Address { get; set; }
    public bool IsBusy { get; set; }
    public DateTime LastSeen { get; set; }
}
