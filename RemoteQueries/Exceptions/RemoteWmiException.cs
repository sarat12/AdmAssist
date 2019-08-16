using System;

namespace RemoteQueries.Exceptions
{
    public class RemoteWmiException : Exception
    {
        public RemoteWmiException()
        {
        }

        public RemoteWmiException(string message) : base(message)
        {
        }

        public RemoteWmiException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
