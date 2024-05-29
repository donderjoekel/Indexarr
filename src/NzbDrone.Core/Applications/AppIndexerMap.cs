using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Applications
{
    public class AppIndexerMap : ModelBase
    {
        public Guid IndexerId { get; set; }
        public Guid AppId { get; set; }
        public Guid RemoteIndexerId { get; set; }
        public string RemoteIndexerName { get; set; }
    }
}
