using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Chapters;
using NzbDrone.Core.IndexedMangas;
using NzbDrone.Core.Mangas;
using Prowlarr.Http;
using Prowlarr.Http.REST;

namespace Prowlarr.Api.V1.Chapters;

[V1ApiController]
public class ChapterController : RestController<ChapterResource>
{
    private readonly IChapterService _chapterService;
    private readonly IIndexedMangaService _indexedMangaService;
    private readonly IMangaService _mangaService;

    public ChapterController(IChapterService chapterService,
        IIndexedMangaService indexedMangaService,
        IMangaService mangaService)
    {
        _chapterService = chapterService;
        _indexedMangaService = indexedMangaService;
        _mangaService = mangaService;
    }

    public override ChapterResource GetResourceById(Guid id)
    {
        throw new NotImplementedException();
    }

    [HttpGet("mangaupdates/{mangaUpdatesId:long}")]
    [AllowAnonymous]
    [Produces("application/json")]
    public List<ChapterResource> GetByMangaUpdatesId(long mangaUpdatesId)
    {
        var manga = _mangaService.GetByMangaUpdatesId(mangaUpdatesId);
        if (manga == null)
        {
            return new List<ChapterResource>();
        }

        var resources = new List<ChapterResource>();
        var indexedMangas = _indexedMangaService.GetByMangaId(manga.Id);

        foreach (var indexedManga in indexedMangas)
        {
            var chapters = _chapterService.GetForIndexedManga(indexedManga.Id);
            resources.AddRange(chapters.Select(c => c.ToResource()));
        }

        return resources.OrderBy(x => x.Volume).ThenBy(x => x.Number).Select((value, i) =>
        {
            value.AbsoluteNumber = i + 1;
            return value;
        }).ToList();
    }

    [HttpGet("anilist/{aniListId:int}")]
    [AllowAnonymous]
    [Produces("application/json")]
    public List<ChapterResource> GetByAniListId(int aniListId)
    {
        var manga = _mangaService.GetByAniListId(aniListId);
        if (manga == null)
        {
            return new List<ChapterResource>();
        }

        var resources = new List<ChapterResource>();
        var indexedMangas = _indexedMangaService.GetByMangaId(manga.Id);

        foreach (var indexedManga in indexedMangas)
        {
            var chapters = _chapterService.GetForIndexedManga(indexedManga.Id);
            resources.AddRange(chapters.Select(c => c.ToResource()));
        }

        return resources.OrderBy(x => x.Volume).ThenBy(x => x.Number).Select((value, i) =>
        {
            value.AbsoluteNumber = i + 1;
            return value;
        }).ToList();
    }
}
