using System;
using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Applications.Sonarr
{
    public class SonarrSettingsValidator : AbstractValidator<SonarrSettings>
    {
        public SonarrSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).IsValidUrl();
            RuleFor(c => c.ProwlarrUrl).IsValidUrl();
            RuleFor(c => c.ApiKey).NotEmpty();
            RuleFor(c => c.SyncCategories).NotEmpty();
        }
    }

    public class SonarrSettings : IApplicationSettings
    {
        private static readonly SonarrSettingsValidator Validator = new ();

        public SonarrSettings()
        {
            ProwlarrUrl = "http://localhost:9696";
            BaseUrl = "http://localhost:8989";
            SyncCategories = new[]
            {
                NewznabStandardCategory.Books.Id,
                NewznabStandardCategory.BooksManga.Id,
                NewznabStandardCategory.BooksManhua.Id,
                NewznabStandardCategory.BooksManhwa.Id
            };
            AnimeSyncCategories = Array.Empty<int>();
        }

        [FieldDefinition(0, Label = "Indexarr Server", HelpText = "Indexarr server URL as Mangarr sees it, including http(s)://, port, and urlbase if needed", Placeholder = "http://localhost:9696")]
        public string ProwlarrUrl { get; set; }

        [FieldDefinition(1, Label = "Mangarr Server", HelpText = "URL used to connect to Mangarr server, including http(s)://, port, and urlbase if required", Placeholder = "http://localhost:8989")]
        public string BaseUrl { get; set; }

        [FieldDefinition(2, Label = "API Key", Privacy = PrivacyLevel.ApiKey, HelpText = "The ApiKey generated by Mangarr in Settings/General")]
        public string ApiKey { get; set; }

        [FieldDefinition(3, Label = "Sync Categories", Type = FieldType.Select, SelectOptions = typeof(NewznabCategoryFieldConverter), Advanced = true, HelpText = "Only Indexers that support these categories will be synced")]
        public IEnumerable<int> SyncCategories { get; set; }

        [FieldDefinition(4, Label = "Anime Sync Categories", Type = FieldType.Select, SelectOptions = typeof(NewznabCategoryFieldConverter), Advanced = true, HelpText = "Only Indexers that support these categories will be synced")]
        public IEnumerable<int> AnimeSyncCategories { get; set; }

        [FieldDefinition(5, Label = "Sync Anime Standard Format Search", Type = FieldType.Checkbox, HelpText = "Sync also searching for anime using the standard numbering", Advanced = true)]
        public bool SyncAnimeStandardFormatSearch { get; set; }

        [FieldDefinition(6, Type = FieldType.Checkbox, Label = "ApplicationSettingsSyncRejectBlocklistedTorrentHashes", HelpText = "ApplicationSettingsSyncRejectBlocklistedTorrentHashesHelpText", Advanced = true)]
        public bool SyncRejectBlocklistedTorrentHashesWhileGrabbing { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
