using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(42)]
    public class indexed_manga_update : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("IndexedManga")
                .AlterColumn("MangaId").AsInt32().Nullable()
                .AddColumn("Title").AsString().NotNullable();
        }
    }
}
