using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Drones;

public interface IDroneRepository : IBasicRepository<Drone>
{
    Drone GetByAddress(string address);
}

public class DroneRepository : BasicRepository<Drone>, IDroneRepository
{
    public DroneRepository(IMainDatabase database, IEventAggregator eventAggregator)
        : base(database, eventAggregator)
    {
    }

    public Drone GetByAddress(string address)
    {
        return Query(x => x.Address == address).FirstOrDefault();
    }
}
