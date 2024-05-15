namespace NzbDrone.Core.Mangas;

public interface IMangaService
{
}

public class MangaService : IMangaService
{
    private readonly IMangaRepository _mangaRepository;

    public MangaService(IMangaRepository mangaRepository)
    {
        _mangaRepository = mangaRepository;
    }
}
