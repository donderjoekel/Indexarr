using System;
using System.Linq;
using System.Net;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Drones.Commands;
using NzbDrone.Core.Drones.Events;
using NzbDrone.Core.Indexing.Commands;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Drones;

public interface IDroneService
{
    bool IsDirector();
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
    public const int RefreshInterval = 1;
    public const int CleanInterval = RefreshInterval * 2;

    private readonly IDroneRepository _droneRepository;
    private readonly IHttpClient _httpClient;
    private readonly Logger _logger;
    private readonly IManageCommandQueue _commandQueue;
    private readonly IEventAggregator _eventAggregator;
    private readonly IConfigFileProvider _configFile;

    public DroneService(IDroneRepository droneRepository,
        IHttpClient httpClient,
        Logger logger,
        IManageCommandQueue commandQueue,
        IEventAggregator eventAggregator,
        IConfigFileProvider configFile)
    {
        _droneRepository = droneRepository;
        _httpClient = httpClient;
        _logger = logger;
        _commandQueue = commandQueue;
        _eventAggregator = eventAggregator;
        _configFile = configFile;
    }

    public bool IsDirector()
    {
        return _configFile.IsDirector;
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

        try
        {
            var droneAddress = drone.Address;
            if (IPAddress.TryParse(droneAddress, out _))
            {
                droneAddress = "http://" + droneAddress;
            }

            var response = _httpClient.Get(new HttpRequest(drone.Address + "/api/v1/drone/index/" + indexerId));
            if (response.HasHttpError)
            {
                _logger.Error("Failed to dispatch partial index to drone {0}", drone.Address);
            }
            else
            {
                drone.IsBusy = true;
                _droneRepository.Update(drone);
            }

            return !response.HasHttpError;
        }
        catch (Exception e)
        {
            _logger.Error(e);
            return false;
        }
    }

    public void DispatchPartialIndexFinished(Guid indexerId)
    {
        var response = _httpClient.Get(new HttpRequest(_configFile.DirectorAddress + "/api/v1/drone/finish/" + indexerId));
        if (response.HasHttpError)
        {
            _logger.Error("Failed to notify director of partial index completion");
        }
    }

    public void RegisterDrone(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            _logger.Error("Invalid address for drone registration");
            return;
        }

        _logger.Info("Registering drone");
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
        if (IsDirector())
        {
            return;
        }

        _logger.Info("Registering self with director");
        var response = _httpClient.Get(
            new HttpRequest(
                _configFile.DirectorAddress + "/api/v1/drone/register/" + _configFile.DroneAddress.ToBase64()));
        if (response.HasHttpError)
        {
            _logger.Error("Failed to register with director");
        }
    }

    public void Execute(RemoveUnresponsiveDronesCommand message)
    {
        if (!IsDirector())
        {
            return;
        }

        _logger.Info("Checking for unresponsive drones");
        var now = DateTime.UtcNow;
        var unresponsiveDrones = _droneRepository.All()
            .Where(x => now - x.LastSeen > TimeSpan.FromMinutes(CleanInterval))
            .ToList();

        _logger.Info("Removing {0} unresponsive drones", unresponsiveDrones.Count);
        _droneRepository.DeleteMany(unresponsiveDrones);
    }
}
