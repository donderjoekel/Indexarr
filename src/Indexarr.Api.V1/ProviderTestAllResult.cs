using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;

namespace Prowlarr.Api.V1
{
    public class ProviderTestAllResult
    {
        public Guid Id { get; set; }
        public bool IsValid => ValidationFailures.Empty();
        public List<ValidationFailure> ValidationFailures { get; set; }

        public ProviderTestAllResult()
        {
            ValidationFailures = new List<ValidationFailure>();
        }
    }
}
