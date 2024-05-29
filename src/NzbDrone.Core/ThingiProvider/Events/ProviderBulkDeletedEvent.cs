using System;
using System.Collections.Generic;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.ThingiProvider.Events
{
    public class ProviderBulkDeletedEvent<TProvider> : IEvent
    {
        public IEnumerable<Guid> ProviderIds { get; private set; }

        public ProviderBulkDeletedEvent(IEnumerable<Guid> ids)
        {
            ProviderIds = ids;
        }
    }
}
