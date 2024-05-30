using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.Definitions.Indexarr;
using NzbDrone.Core.Indexers.Settings;

namespace NzbDrone.Core.Indexers.Definitions.BuddyComplex;

public class BuddyComplexRequestGenerator : IndexarrRequestGenerator
{
    private readonly NoAuthTorrentBaseSettings _settings;

    public BuddyComplexRequestGenerator(NoAuthTorrentBaseSettings settings)
    {
        _settings = settings;
    }

    protected override IndexerRequest GetFullIndexRequest()
    {
        return new IndexerRequest(_settings.BaseUrl, HttpAccept.Html);
    }

    protected override IndexerRequest GetTestIndexRequest()
    {
        return new IndexerRequest(_settings.BaseUrl, HttpAccept.Html);
    }
}
