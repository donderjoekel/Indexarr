using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(43)]
    public class table_name_update : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Rename.Table("Manga").To("Mangas");
            Rename.Table("IndexedManga").To("IndexedMangas");
            Rename.Table("Chapter").To("Chapters");
        }
    }
}
