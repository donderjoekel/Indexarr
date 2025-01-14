using System;
using Microsoft.AspNetCore.Mvc;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Drones;
using Prowlarr.Http;
using Prowlarr.Http.REST;

namespace Prowlarr.Api.V1.Drone;

[V1ApiController]
public class DroneController : RestController<DroneResource>
{
    private readonly IDroneService _droneService;
    private readonly Logger _logger;

    public DroneController(IDroneService droneService, Logger logger)
    {
        _droneService = droneService;
        _logger = logger;
    }

    public override DroneResource GetResourceById(Guid id)
    {
        throw new NotImplementedException();
    }

    [HttpGet("register/{address}")]
    public IActionResult RegisterDrone(string address)
    {
        var actualAddress = address.FromBase64();
        _logger.Info("Drone registration incoming from {0}", actualAddress);
        _droneService.RegisterDrone(actualAddress);
        return Ok();
    }

    [HttpGet("index/{indexerId}")]
    public IActionResult StartIndex(string indexerId)
    {
        _logger.Info("Index request for {0} received", indexerId);
        _droneService.StartPartialIndex(indexerId);
        return Ok();
    }

    [HttpGet("finish/{address}/{indexerId}")]
    public IActionResult FinishIndex(string address, string indexerId)
    {
        _logger.Info("Index request for {0} finished", indexerId);
        _droneService.FinishPartialIndex(address.FromBase64(), indexerId);
        return Ok();
    }
}
