using System;
using NzbDrone.Core.Chapters;
using Prowlarr.Http.REST;

namespace Prowlarr.Api.V1.Chapters;

public class ChapterResource : RestResource
{
    public int AbsoluteNumber { get; set; }
    public int Volume { get; set; }
    public decimal Number { get; set; }
    public DateTime Date { get; set; }
}

public static class ChapterResourceMapper
{
    public static ChapterResource ToResource(this Chapter model)
    {
        return new ChapterResource
        {
            Id = model.Id,
            Volume = model.Volume,
            Number = model.Number,
            Date = model.Date
        };
    }
}
