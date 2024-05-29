using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Drone;
using Prowlarr.Http;
using Prowlarr.Http.Extensions;
using Prowlarr.Http.REST;

namespace Prowlarr.Api.V1.Drone;

[V1ApiController]
public class DroneController : RestController<DroneResource>
{
    private readonly IDroneService _droneService;

    public DroneController(IDroneService droneService)
    {
        _droneService = droneService;
    }

    public override DroneResource GetResourceById(Guid id)
    {
        throw new NotImplementedException();
    }

    [HttpGet("register")]
    [AllowAnonymous]
    public IActionResult RegisterDrone()
    {
        _droneService.RegisterDrone(HttpContext.GetRemoteIP());
        return Ok();
    }

    [HttpGet("index/{indexerId}")]
    [AllowAnonymous]
    public IActionResult StartIndex(string indexerId)
    {
        _droneService.StartPartialIndex(indexerId);
        return Ok();
    }

    [HttpGet("finish/{indexerId}")]
    [AllowAnonymous]
    public IActionResult FinishIndex(string indexerId)
    {
        _droneService.FinishPartialIndex(indexerId);
        return Ok();
    }
}
