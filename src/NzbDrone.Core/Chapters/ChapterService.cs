using NzbDrone.Core.IndexedMangas;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Chapters;

public interface IChapterService
{
    Chapter AddOrUpdate(IndexedManga indexedManga, ChapterInfo chapterInfo);
}

public class ChapterService : IChapterService
{
    private readonly IChapterRepository _chapterRepository;

    public ChapterService(IChapterRepository chapterRepository)
    {
        _chapterRepository = chapterRepository;
    }

    public Chapter AddOrUpdate(IndexedManga indexedManga, ChapterInfo chapterInfo)
    {
        var chapter = _chapterRepository.GetForIndexerByChapterNumber(indexedManga.IndexerId, chapterInfo.Number);

        if (chapter == null)
        {
            chapter = _chapterRepository.Insert(new Chapter()
            {
                IndexedMangaId = indexedManga.IndexerId,
                Number = chapterInfo.Number,
                Url = chapterInfo.Url,
                Date = chapterInfo.Date
            });
        }
        else
        {
            chapter.Url = chapterInfo.Url;
            chapter.Date = chapterInfo.Date;
            chapter = _chapterRepository.Update(chapter);
        }

        return chapter;
    }
}
