using System.Text;
using System.Threading.Tasks;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexer : IProvider
    {
        bool SupportsRss { get; }
        bool SupportsSearch { get; }
        bool SupportsRedirect { get; }
        bool SupportsPagination { get; }
        IndexerCapabilities Capabilities { get; }

        string[] IndexerUrls { get; }
        string[] LegacyUrls { get; }
        string Description { get; }
        Encoding Encoding { get; }
        string Language { get; }
        DownloadProtocol Protocol { get; }
        IndexerPrivacy Privacy { get; }

        Task<IndexerPageableIndexResult> FullIndex();

        bool IsObsolete();

        IndexerCapabilities GetCapabilities();
    }
}
