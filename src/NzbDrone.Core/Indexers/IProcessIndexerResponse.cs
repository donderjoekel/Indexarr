using System;
using System.Collections.Generic;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers
{
    public interface IParseIndexerResponse
    {
        IList<MangaInfo> ParseResponse(IndexerResponse indexerResponse);
        Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }
    }
}
