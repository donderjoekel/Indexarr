using System;
using System.Collections.Generic;
using NzbDrone.Core.IndexedMangas.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.IndexedMangas;

public interface IIndexedMangaService
{
    IndexedManga Find(Guid id);
    IEnumerable<IndexedManga> FindByTitle(string title);
    IEnumerable<IndexedManga> All();
    IEnumerable<IndexedManga> GetByMangaId(Guid mangaId);
    bool Exists(MangaInfo mangaInfo);
    IndexedManga Create(MangaInfo mangaInfo);
    IndexedManga Update(MangaInfo mangaInfo);
    IEnumerable<IndexedManga> GetWithoutLinkedManga();
    void LinkToManga(Guid indexedMangaId, Guid mangaId);
}

public class IndexedMangaService : IIndexedMangaService
{
    private readonly IIndexedMangaRepository _indexedMangaRepository;
    private readonly IEventAggregator _eventAggregator;

    public IndexedMangaService(IIndexedMangaRepository indexedMangaRepository, IEventAggregator eventAggregator)
    {
        _indexedMangaRepository = indexedMangaRepository;
        _eventAggregator = eventAggregator;
    }

    public IndexedManga Find(Guid id)
    {
        return _indexedMangaRepository.Get(id);
    }

    public IEnumerable<IndexedManga> FindByTitle(string title)
    {
        return _indexedMangaRepository.GetWithTitle(title);
    }

    public IEnumerable<IndexedManga> All()
    {
        return _indexedMangaRepository.All();
    }

    public IEnumerable<IndexedManga> GetByMangaId(Guid mangaId)
    {
        return _indexedMangaRepository.GetByMangaId(mangaId);
    }

    public bool Exists(MangaInfo mangaInfo)
    {
        _ = mangaInfo ?? throw new ArgumentNullException(nameof(mangaInfo));
        return _indexedMangaRepository.GetByUrl(mangaInfo.Url) != null;
    }

    public IndexedManga Create(MangaInfo mangaInfo)
    {
        _ = mangaInfo ?? throw new ArgumentNullException(nameof(mangaInfo));

        var indexedManga = _indexedMangaRepository.Insert(new IndexedManga
        {
            Url = mangaInfo.Url,
            Title = mangaInfo.Title,
            IndexerId = mangaInfo.IndexerId
        });
        _eventAggregator.PublishEvent(new IndexedMangaCreatedEvent(indexedManga));
        return indexedManga;
    }

    public IndexedManga Update(MangaInfo mangaInfo)
    {
        _ = mangaInfo ?? throw new ArgumentNullException(nameof(mangaInfo));

        var indexedManga = _indexedMangaRepository.GetByUrl(mangaInfo.Url);
        var changed = false;
        if (indexedManga.Title != mangaInfo.Title)
        {
            indexedManga.Title = mangaInfo.Title;
            changed = true;
        }

        if (changed)
        {
            indexedManga = _indexedMangaRepository.Update(indexedManga);
            _eventAggregator.PublishEvent(new IndexedMangaTitleUpdatedEvent(indexedManga));
        }

        return indexedManga;
    }

    public IEnumerable<IndexedManga> GetWithoutLinkedManga()
    {
        return _indexedMangaRepository.GetWithoutLinkedManga();
    }

    public void LinkToManga(Guid indexedMangaId, Guid mangaId)
    {
        var indexedManga = _indexedMangaRepository.Get(indexedMangaId);
        indexedManga.MangaId = mangaId;
        _indexedMangaRepository.Update(indexedManga);
        _eventAggregator.PublishEvent(new IndexedMangaLinkedEvent(indexedManga));
    }
}
