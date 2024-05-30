using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Parser.Model;

public class MangaInfo
{
    public MangaInfo()
    {
        Chapters = new List<ChapterInfo>();
    }

    public Guid IndexerId { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }
    public List<ChapterInfo> Chapters { get; set; }
}
