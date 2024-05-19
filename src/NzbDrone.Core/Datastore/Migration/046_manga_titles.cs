using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(46)]
    public class manga_titles : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("Titles").FromTable("Mangas");
            Create.Column("MangaUpdatesTitles").OnTable("Mangas").AsString().NotNullable();
            Create.Column("AniListTitles").OnTable("Mangas").AsString().NotNullable();
            Create.Column("MyAnimeListTitles").OnTable("Mangas").AsString().NotNullable();
        }
    }
}
