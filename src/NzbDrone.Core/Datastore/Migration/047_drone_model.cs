using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(47)]
    public class drone_model : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("Drones")
                .WithColumn("Address").AsString().NotNullable()
                .WithColumn("IsBusy").AsBoolean().NotNullable()
                .WithColumn("LastSeen").AsDateTime().NotNullable();
        }
    }
}
