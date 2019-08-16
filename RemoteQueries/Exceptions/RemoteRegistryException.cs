using System;

namespace RemoteQueries.Exceptions
{
    public class RemoteRegistryException : Exception
    {
        public RemoteRegistryException()
        {
            
        }

        public RemoteRegistryException(string message) : base(message)
        {
        }

        public RemoteRegistryException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
