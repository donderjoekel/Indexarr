using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Configuration;
using NzbDrone.Http.REST.Attributes;
using Prowlarr.Http.REST;

namespace Prowlarr.Api.V1.Config
{
    public abstract class ConfigController<TResource> : RestController<TResource>
        where TResource : RestResource, new()
    {
        protected readonly IConfigService _configService;

        protected ConfigController(IConfigService configService)
        {
            _configService = configService;
        }

        public override TResource GetResourceById(Guid id)
        {
            return GetConfig();
        }

        [HttpGet]
        [Produces("application/json")]
        public TResource GetConfig()
        {
            var resource = ToResource(_configService);
            resource.Id = Guid.NewGuid();

            return resource;
        }

        [RestPutById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public virtual ActionResult<TResource> SaveConfig([FromBody] TResource resource)
        {
            var dictionary = resource.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(resource, null));

            _configService.SaveConfigDictionary(dictionary);

            return Accepted(resource.Id);
        }

        protected abstract TResource ToResource(IConfigService model);
    }
}
