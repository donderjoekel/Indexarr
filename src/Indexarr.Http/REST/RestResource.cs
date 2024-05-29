using System;
using System.Text.Json.Serialization;

namespace Prowlarr.Http.REST
{
    public abstract class RestResource
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public virtual Guid Id { get; set; }

        [JsonIgnore]
        public virtual string ResourceName => GetType().Name.ToLowerInvariant().Replace("resource", "");
    }
}
