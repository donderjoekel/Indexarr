using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Profiles
{
    public class ProfileInUseException : NzbDroneException
    {
        public ProfileInUseException(Guid profileId)
            : base("Profile [{0}] is in use.", profileId)
        {
        }
    }
}
