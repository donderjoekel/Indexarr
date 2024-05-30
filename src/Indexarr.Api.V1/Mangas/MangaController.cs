using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Mangas;
using Prowlarr.Http;
using Prowlarr.Http.REST;

namespace Prowlarr.Api.V1.Mangas;

[V1ApiController]
public class MangaController : RestController<MangaResource>
{
    private readonly IMangaService _mangaService;

    public MangaController(IMangaService mangaService)
    {
        _mangaService = mangaService;
    }

    public override MangaResource GetResourceById(Guid id)
    {
        return _mangaService.Find(id).ToResource();
    }

    [HttpGet]
    [AllowAnonymous]
    [Produces("application/json")]
    public List<MangaResource> Get()
    {
        return _mangaService.All().SelectList(x => x.ToResource());
    }
}
