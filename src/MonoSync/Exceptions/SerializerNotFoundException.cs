using System;

namespace MonoSync.Exceptions
{
    public class SerializerNotFoundException : MonoSyncException
    {
        public SerializerNotFoundException(Type type) : base(
            $"Could not find {nameof(ISerializer)} for {type}")
        {
        }
    }
}