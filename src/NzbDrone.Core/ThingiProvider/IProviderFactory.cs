using System;
using System.Collections.Generic;
using FluentValidation.Results;

namespace NzbDrone.Core.ThingiProvider
{
    public interface IProviderFactory<TProvider, TProviderDefinition>
        where TProviderDefinition : ProviderDefinition, new()
        where TProvider : IProvider
    {
        List<TProviderDefinition> All();
        List<TProvider> GetAvailableProviders();
        bool Exists(Guid id);
        TProviderDefinition Find(Guid id);
        TProviderDefinition Get(Guid id);
        IEnumerable<TProviderDefinition> Get(IEnumerable<Guid> ids);
        TProviderDefinition Create(TProviderDefinition definition);
        void Update(TProviderDefinition definition);
        IEnumerable<TProviderDefinition> Update(IEnumerable<TProviderDefinition> definitions);
        void Delete(Guid id);
        void Delete(IEnumerable<Guid> ids);
        IEnumerable<TProviderDefinition> GetDefaultDefinitions();
        IEnumerable<TProviderDefinition> GetPresetDefinitions(TProviderDefinition providerDefinition);
        void SetProviderCharacteristics(TProviderDefinition definition);
        void SetProviderCharacteristics(TProvider provider, TProviderDefinition definition);
        TProvider GetInstance(TProviderDefinition definition);
        ValidationResult Test(TProviderDefinition definition);
        object RequestAction(TProviderDefinition definition, string action, IDictionary<string, string> query);
        List<TProviderDefinition> AllForTag(Guid tagId);
    }
}
