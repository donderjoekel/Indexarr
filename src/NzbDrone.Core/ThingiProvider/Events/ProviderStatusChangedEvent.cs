using System;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.ThingiProvider.Events
{
    public class ProviderStatusChangedEvent<TProvider> : IEvent
    {
        public Guid ProviderId { get; private set; }

        public ProviderStatusBase Status { get; private set; }

        public ProviderStatusChangedEvent(Guid id, ProviderStatusBase status)
        {
            ProviderId = id;
            Status = status;
        }
    }
}
