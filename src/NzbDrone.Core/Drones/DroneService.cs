using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Drones.Builders;
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
    void FinishPartialIndex(string address, string indexerId);
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
    private readonly IDirectorRequestBuilder _directorRequestBuilder;
    private readonly IDroneRequestBuilder _droneRequestBuilder;

    public DroneService(IDroneRepository droneRepository,
        IHttpClient httpClient,
        Logger logger,
        IManageCommandQueue commandQueue,
        IEventAggregator eventAggregator,
        IConfigFileProvider configFile,
        IDirectorRequestBuilder directorRequestBuilder,
        IDroneRequestBuilder droneRequestBuilder)
    {
        _droneRepository = droneRepository;
        _httpClient = httpClient;
        _logger = logger;
        _commandQueue = commandQueue;
        _eventAggregator = eventAggregator;
        _configFile = configFile;
        _directorRequestBuilder = directorRequestBuilder;
        _droneRequestBuilder = droneRequestBuilder;
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
            _logger.Info("Requesting index on drone {0}", drone.Address);

            var request = _droneRequestBuilder.Create(drone)
                .Resource("index/{indexer}")
                .SetSegment("indexer", indexerId.ToString())
                .Build();

            var response = _httpClient.Get(request);
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
        var request = _directorRequestBuilder.Builder
            .Resource("finish/{address}/{indexer}")
            .SetSegment("address", _configFile.DroneAddress.ToBase64())
            .SetSegment("indexer", indexerId.ToString())
            .Build();

        var response = _httpClient.Get(request);
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

    public void FinishPartialIndex(string address, string indexerId)
    {
        var drone = _droneRepository.GetByAddress(address);
        drone.IsBusy = false;
        _droneRepository.Update(drone);

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

        var request = _directorRequestBuilder.Builder
            .Resource("register/{address}")
            .SetSegment("address", _configFile.DroneAddress.ToBase64())
            .Build();

        var response = _httpClient.Get(request);
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

        foreach (var unresponsiveDrone in unresponsiveDrones)
        {
            _droneRepository.Delete(unresponsiveDrone);
        }
    }
}
