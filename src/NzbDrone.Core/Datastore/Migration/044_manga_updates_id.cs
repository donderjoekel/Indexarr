using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(44)]
    public class manga_updates_id : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Mangas").AlterColumn("MangaUpdatesId").AsInt64().Nullable();
        }
    }
}
