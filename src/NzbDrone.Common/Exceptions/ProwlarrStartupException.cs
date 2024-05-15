using System;

namespace NzbDrone.Common.Exceptions
{
    public class ProwlarrStartupException : NzbDroneException
    {
        public ProwlarrStartupException(string message, params object[] args)
            : base("Indexarr failed to start: " + string.Format(message, args))
        {
        }

        public ProwlarrStartupException(string message)
            : base("Indexarr failed to start: " + message)
        {
        }

        public ProwlarrStartupException()
            : base("Indexarr failed to start")
        {
        }

        public ProwlarrStartupException(Exception innerException, string message, params object[] args)
            : base("Indexarr failed to start: " + string.Format(message, args), innerException)
        {
        }

        public ProwlarrStartupException(Exception innerException, string message)
            : base("Indexarr failed to start: " + message, innerException)
        {
        }

        public ProwlarrStartupException(Exception innerException)
            : base("Indexarr failed to start: " + innerException.Message)
        {
        }
    }
}
