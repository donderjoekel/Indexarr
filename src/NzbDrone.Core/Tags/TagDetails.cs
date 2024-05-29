using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Tags
{
    public class TagDetails : ModelBase
    {
        public string Label { get; set; }
        public List<Guid> NotificationIds { get; set; }
        public List<Guid> IndexerIds { get; set; }
        public List<Guid> IndexerProxyIds { get; set; }
        public List<Guid> ApplicationIds { get; set; }

        public bool InUse => NotificationIds.Any() || IndexerIds.Any() || IndexerProxyIds.Any() || ApplicationIds.Any();
    }
}
