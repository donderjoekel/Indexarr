using System;
using System.Collections.Generic;
using NzbDrone.Core.ThingiProvider;

namespace Prowlarr.Api.V1
{
    public class ProviderBulkResource<T>
    {
        public List<Guid> Ids { get; set; }
        public List<Guid> Tags { get; set; }
        public ApplyTags ApplyTags { get; set; }

        public ProviderBulkResource()
        {
            Ids = new List<Guid>();
        }
    }

    public enum ApplyTags
    {
        Add,
        Remove,
        Replace
    }

    public class ProviderBulkResourceMapper<TProviderBulkResource, TProviderDefinition>
        where TProviderBulkResource : ProviderBulkResource<TProviderBulkResource>, new()
        where TProviderDefinition : ProviderDefinition, new()
    {
        public virtual List<TProviderDefinition> UpdateModel(TProviderBulkResource resource, List<TProviderDefinition> existingDefinitions)
        {
            if (resource == null)
            {
                return new List<TProviderDefinition>();
            }

            return existingDefinitions;
        }
    }
}
