using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Drone.Commands;
using NzbDrone.Core.Drone.Events;
using NzbDrone.Core.Indexing.Commands;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Drone;

public interface IDroneService
{
    bool IsMainDrone();
    int GetDroneCount();
    void RegisterDrone(string address);
    bool DispatchPartialIndex(Guid indexerId);
    void DispatchPartialIndexFinished(Guid indexerId);
    void StartPartialIndex(string indexerId);
    void FinishPartialIndex(string indexerId);
}

public class DroneService : IDroneService,
    IExecute<RegisterDroneCommand>,
    IExecute<RemoveUnresponsiveDronesCommand>
{
    public const int RefreshInterval = 60;
    public const int CleanInterval = RefreshInterval * 2;

    private readonly IDroneRepository _droneRepository;
    private readonly IHttpClient _httpClient;
    private readonly Logger _logger;
    private readonly IManageCommandQueue _commandQueue;
    private readonly IEventAggregator _eventAggregator;

    private readonly string _hostAddress;

    public DroneService(IDroneRepository droneRepository,
        IHttpClient httpClient,
        Logger logger,
        IManageCommandQueue commandQueue,
        IEventAggregator eventAggregator)
    {
        _droneRepository = droneRepository;
        _httpClient = httpClient;
        _logger = logger;
        _commandQueue = commandQueue;
        _eventAggregator = eventAggregator;

        _hostAddress = Environment.GetEnvironmentVariable("HOST_ADDRESS");
    }

    public bool IsMainDrone()
    {
        return !string.IsNullOrWhiteSpace(_hostAddress);
    }

    public int GetDroneCount()
    {
        return _droneRepository.Count();
    }

    public bool DispatchPartialIndex(Guid indexerId)
    {
        var drone = _droneRepository.All().FirstOrDefault(x => !x.IsBusy);

        if (drone == null)
        {
            throw new Exception("No drone available");
        }

        drone.IsBusy = true;
        _droneRepository.Update(drone);

        try
        {
            var response = _httpClient.Get(new HttpRequest(drone.Address + "/api/v1/drone/index/" + indexerId));
            return !response.HasHttpError;
        }
        catch (Exception e)
        {
            drone.IsBusy = false;
            _droneRepository.Update(drone);
            _logger.Error(e);
            return false;
        }
    }

    public void DispatchPartialIndexFinished(Guid indexerId)
    {
        _httpClient.Get(new HttpRequest(_hostAddress + "/api/v1/drone/finish/" + indexerId));
    }

    public void RegisterDrone(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            _logger.Error("Invalid address for drone registration");
            return;
        }

        var drone = _droneRepository.GetByAddress(address);
        if (drone == null)
        {
            _droneRepository.Insert(
                new Drone
                {
                    Address = address,
                    LastSeen = DateTime.UtcNow
                });
        }
        else
        {
            drone.LastSeen = DateTime.UtcNow;
            _droneRepository.Update(drone);
        }
    }

    public void StartPartialIndex(string indexerId)
    {
        var command = new PartialIndexCommand()
        {
            IndexerId = Guid.Parse(indexerId)
        };

        _commandQueue.Push(command);
    }

    public void FinishPartialIndex(string indexerId)
    {
        _eventAggregator.PublishEvent(
            new PartialIndexFinishedEvent()
            {
                IndexerId = indexerId
            });
    }

    public void Execute(RegisterDroneCommand message)
    {
        if (IsMainDrone())
        {
            return;
        }

        _httpClient.Get(new HttpRequest(_hostAddress + "/api/v1/drone/register"));
    }

    public void Execute(RemoveUnresponsiveDronesCommand message)
    {
        if (!IsMainDrone())
        {
            return;
        }

        var now = DateTime.UtcNow;
        var unresponsiveDrones = _droneRepository.All()
            .Where(x => now - x.LastSeen > TimeSpan.FromSeconds(CleanInterval))
            .ToList();

        _droneRepository.DeleteMany(unresponsiveDrones);
    }
}
