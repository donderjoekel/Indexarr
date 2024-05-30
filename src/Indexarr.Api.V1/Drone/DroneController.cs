using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using NzbDrone.Core.Drones;
using Prowlarr.Http;
using Prowlarr.Http.Extensions;
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

    [HttpGet("register")]
    [AllowAnonymous]
    public IActionResult RegisterDrone()
    {
        _logger.Info(
            "Drone registration incoming from {0}:{1}",
            HttpContext.GetRemoteIP(),
            HttpContext.Connection.RemotePort);
        _droneService.RegisterDrone(HttpContext.GetRemoteIP(), HttpContext.Connection.RemotePort);
        return Ok();
    }

    [HttpGet("index/{indexerId}")]
    [AllowAnonymous]
    public IActionResult StartIndex(string indexerId)
    {
        _logger.Info("Index request for {0} received", indexerId);
        _droneService.StartPartialIndex(indexerId);
        return Ok();
    }

    [HttpGet("finish/{indexerId}")]
    [AllowAnonymous]
    public IActionResult FinishIndex(string indexerId)
    {
        _logger.Info("Index request for {0} finished", indexerId);
        _droneService.FinishPartialIndex(indexerId);
        return Ok();
    }
}
