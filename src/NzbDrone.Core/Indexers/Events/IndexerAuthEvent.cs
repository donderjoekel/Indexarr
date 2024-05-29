using System;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Indexers.Events
{
    public class IndexerAuthEvent : IEvent
    {
        public Guid IndexerId { get; set; }
        public bool Successful { get; set; }
        public long Time { get; set; }

        public IndexerAuthEvent(Guid indexerId, bool successful, long time)
        {
            IndexerId = indexerId;
            Successful = successful;
            Time = time;
        }
    }
}
