using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(41)]
    public class indexarr_init : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("Manga")
                .WithColumn("MangaUpdatesId").AsInt32().Nullable()
                .WithColumn("MyAnimeListId").AsInt32().Nullable()
                .WithColumn("AniListId").AsInt32().Nullable()
                .WithColumn("Titles").AsString().NotNullable();

            Create.TableForModel("IndexedManga")
                .WithColumn("MangaId").AsGuid().NotNullable()
                .WithColumn("IndexerId").AsGuid().NotNullable()
                .WithColumn("Url").AsString().NotNullable();

            Create.TableForModel("Chapter")
                .WithColumn("IndexedMangaId").AsGuid().NotNullable()
                .WithColumn("Number").AsDecimal().NotNullable()
                .WithColumn("Url").AsString().NotNullable()
                .WithColumn("Date").AsDateTime().NotNullable();
        }
    }
}
