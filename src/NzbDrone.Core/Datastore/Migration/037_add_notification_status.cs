using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(037)]
    public class add_notification_status : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("NotificationStatus")
                  .WithColumn("ProviderId").AsGuid().NotNullable().Unique()
                  .WithColumn("InitialFailure").AsDateTimeOffset().Nullable()
                  .WithColumn("MostRecentFailure").AsDateTimeOffset().Nullable()
                  .WithColumn("EscalationLevel").AsInt32().NotNullable()
                  .WithColumn("DisabledTill").AsDateTimeOffset().Nullable();
        }
    }
}
