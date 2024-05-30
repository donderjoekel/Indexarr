using System;
using System.Diagnostics;

namespace NzbDrone.Core.Datastore
{
    [DebuggerDisplay("{GetType()} ID = {Id}")]
    public abstract class ModelBase
    {
        public Guid Id { get; set; }
    }
}
