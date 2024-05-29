using System;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.ThingiProvider.Events
{
    public class ProviderDeletedEvent<TProvider> : IEvent
    {
        public Guid ProviderId { get; private set; }

        public ProviderDeletedEvent(Guid id)
        {
            ProviderId = id;
        }
    }
}
