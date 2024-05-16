using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public abstract class IndexerBase<TSettings> : IIndexer
        where TSettings : IIndexerSettings, new()
    {
        protected readonly IIndexerStatusService _indexerStatusService;
        protected readonly IConfigService _configService;
        protected readonly Logger _logger;

        public abstract string Name { get; }
        public abstract string[] IndexerUrls { get; }
        public abstract string[] LegacyUrls { get; }
        public abstract string Description { get; }
        public abstract Encoding Encoding { get; }
        public abstract string Language { get; }
        public abstract bool FollowRedirect { get; }
        public abstract DownloadProtocol Protocol { get; }
        public abstract IndexerPrivacy Privacy { get; }
        public int Priority { get; set; }
        public bool Redirect { get; set; }

        public abstract bool SupportsRss { get; }
        public abstract bool SupportsSearch { get; }
        public abstract bool SupportsRedirect { get; }
        public abstract bool SupportsPagination { get; }
        public abstract IndexerCapabilities Capabilities { get; protected set; }

        public IndexerBase(IIndexerStatusService indexerStatusService, IConfigService configService, Logger logger)
        {
            _indexerStatusService = indexerStatusService;
            _configService = configService;
            _logger = logger;
        }

        public Type ConfigContract => typeof(TSettings);

        public bool IsObsolete()
        {
            var attributes = GetType().GetCustomAttributes(false);

            return attributes.OfType<ObsoleteAttribute>().Any();
        }

        public virtual ProviderMessage Message => null;

        public virtual IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                var config = (IProviderConfig)new TSettings();

                yield return new IndexerDefinition
                {
                    Name = Name ?? GetType().Name,
                    Enable = config.Validate().IsValid && SupportsRss,
                    Implementation = GetType().Name,
                    Settings = config
                };
            }
        }

        public virtual ProviderDefinition Definition { get; set; }

        public virtual object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "getUrls")
            {
                var links = IndexerUrls;

                return new
                {
                    options = links.Select(d => new { Value = d, Name = d })
                };
            }

            return null;
        }

        protected TSettings Settings => GetDefaultBaseUrl((TSettings)Definition.Settings);

        public abstract Task<IndexerPageableIndexResult> FullIndex();

        public abstract IndexerCapabilities GetCapabilities();

        protected virtual IList<MangaInfo> CleanupReleases(IEnumerable<MangaInfo> mangas)
        {
            var cleaned = mangas.ToList();

            foreach (var mangaInfo in cleaned)
            {
                mangaInfo.IndexerId = Definition.Id;
            }

            return cleaned;
        }

        protected virtual TSettings GetDefaultBaseUrl(TSettings settings)
        {
            var defaultLink = IndexerUrls.FirstOrDefault();

            if (settings.BaseUrl.IsNullOrWhiteSpace() && defaultLink.IsNotNullOrWhiteSpace())
            {
                settings.BaseUrl = defaultLink;
            }
            else if (settings.BaseUrl.IsNotNullOrWhiteSpace() && LegacyUrls.Contains(settings.BaseUrl))
            {
                _logger.Debug(string.Format("Changing legacy site link from {0} to {1}", settings.BaseUrl, defaultLink));
                settings.BaseUrl = defaultLink;
            }

            return settings;
        }

        public ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            try
            {
                Test(failures).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Test aborted due to exception");
                failures.Add(new ValidationFailure(string.Empty, "Test was aborted due to an error: " + ex.Message));
            }

            return new ValidationResult(failures);
        }

        protected abstract Task Test(List<ValidationFailure> failures);

        public override string ToString()
        {
            return Definition.Name;
        }
    }
}
