using System;

namespace RemoteQueries.Exceptions
{
    public class RemoteSnmpException : Exception
    {
        public RemoteSnmpException()
        {
        }

        public RemoteSnmpException(string message) : base(message)
        {
        }

        public RemoteSnmpException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
