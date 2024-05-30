using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.Definitions.Indexarr;
using NzbDrone.Core.Indexers.Settings;

namespace NzbDrone.Core.Indexers.Definitions.NepNep;

public class NepNepRequestGenerator : IndexarrRequestGenerator
{
    private readonly NoAuthTorrentBaseSettings _settings;

    public NepNepRequestGenerator(NoAuthTorrentBaseSettings settings)
    {
        _settings = settings;
    }

    protected override IndexerRequest GetFullIndexRequest()
    {
        return new IndexarrRequest(_settings.BaseUrl + "search/", HttpAccept.Html, false);
    }

    protected override IndexerRequest GetTestIndexRequest()
    {
        return new IndexarrRequest(_settings.BaseUrl + "search/", HttpAccept.Html, true);
    }
}
