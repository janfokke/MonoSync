using System;

namespace MonoSync.Exceptions
{
    public class MonoSyncException : Exception
    {
        public MonoSyncException(string message) : base(message)
        {
        }
    }
}