using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Profiles
{
    public interface IAppProfileRepository : IBasicRepository<AppSyncProfile>
    {
        bool Exists(Guid id);
    }

    public class AppSyncProfileRepository : BasicRepository<AppSyncProfile>, IAppProfileRepository
    {
        public AppSyncProfileRepository(IMainDatabase database,
                                 IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        protected override List<AppSyncProfile> Query(SqlBuilder builder)
        {
            var profiles = base.Query(builder);

            return profiles;
        }

        public bool Exists(Guid id)
        {
            return Query(x => x.Id == id).Count == 1;
        }
    }
}
