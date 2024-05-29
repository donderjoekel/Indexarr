using System;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Indexing.Commands;

public class PartialIndexCommand : Command
{
    public Guid IndexerId { get; set; }
}
