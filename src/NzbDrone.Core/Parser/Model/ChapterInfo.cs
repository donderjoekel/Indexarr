using System;

namespace NzbDrone.Core.Parser.Model;

public class ChapterInfo
{
    public int Volume { get; set; }
    public decimal Number { get; set; }
    public string Url { get; set; }
    public DateTime Date { get; set; }
}
