using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.IndexedMangas;
using Prowlarr.Http;
using Prowlarr.Http.REST;

namespace Prowlarr.Api.V1.IndexedMangas;

[V1ApiController]
public class IndexedMangaController : RestController<IndexedMangaResource>
{
    private readonly IIndexedMangaService _indexedMangaService;

    public IndexedMangaController(IIndexedMangaService indexedMangaService)
    {
        _indexedMangaService = indexedMangaService;
    }

    public override IndexedMangaResource GetResourceById(Guid id)
    {
        return _indexedMangaService.Find(id).ToResource();
    }

    [HttpGet]
    [AllowAnonymous]
    [Produces("application/json")]
    public List<IndexedMangaResource> Get()
    {
        return _indexedMangaService.All().SelectList(x => x.ToResource());
    }
}
