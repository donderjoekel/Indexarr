namespace NzbDrone.Core.Chapters;

public interface IChapterService
{
}

public class ChapterService : IChapterService
{
    private readonly IChapterRepository _chapterRepository;

    public ChapterService(IChapterRepository chapterRepository)
    {
        _chapterRepository = chapterRepository;
    }
}
