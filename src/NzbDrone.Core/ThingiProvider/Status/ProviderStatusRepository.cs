using System;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.ThingiProvider.Status
{
    public interface IProviderStatusRepository<TModel> : IBasicRepository<TModel>
        where TModel : ProviderStatusBase, new()
    {
        TModel FindByProviderId(Guid providerId);
    }

    public class ProviderStatusRepository<TModel> : BasicRepository<TModel>, IProviderStatusRepository<TModel>
        where TModel : ProviderStatusBase, new()
    {
        public ProviderStatusRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public TModel FindByProviderId(Guid providerId)
        {
            return Query(x => x.ProviderId == providerId).SingleOrDefault();
        }
    }
}
