using System.Collections.Generic;
using System.Net.Http;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.Definitions.Indexarr;
using NzbDrone.Core.Indexers.Settings;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Indexers.Definitions.Madara;

public class MadaraRequestGenerator : IndexarrRequestGenerator
{
    private readonly NoAuthTorrentBaseSettings _settings;

    public MadaraRequestGenerator(NoAuthTorrentBaseSettings settings)
    {
        _settings = settings;
    }

    protected override IndexerRequest GetFullIndexRequest()
    {
        return GetPageRequest(0);
    }

    public IndexarrRequest GetPageRequest(int page)
    {
        var httpRequest = new HttpRequest(_settings.BaseUrl + "/wp-admin/admin-ajax.php");
        httpRequest.Method = HttpMethod.Post;
        httpRequest.Headers.ContentType = "application/x-www-form-urlencoded";
        httpRequest.AllowAutoRedirect = true;

        var data = new Dictionary<string, string>()
        {
            { "action", "madara_load_more" },
            { "page", page.ToString() },
            { "template", "madara-core/content/content-archive" },
            { "vars[paged]", "1" },
            { "vars[posts_per_page]", "100" },
            { "vars[orderby]", "post_title" },
            { "vars[template]", "archive" },
            { "vars[post_type]", "wp-manga" },
            { "vars[post_status]", "publish" },
            { "vars[order]", "ASC" },
            { "vars[meta_query][relation]", "AND" },
            { "vars[manga_archives_item_layout]", "big_thumbnail" },
        };

        httpRequest.SetContent(data.GetQueryString());
        return new IndexarrRequest(httpRequest);
    }

    protected override IndexerRequest GetTestIndexRequest()
    {
        var request = GetPageRequest(0);
        request.IsTest = true;
        return request;
    }
}
