using System;

namespace MonoSync.Exceptions
{
    public class TypeAlreadyRegisteredException : MonoSyncException
    {
        public TypeAlreadyRegisteredException(Type type) : base($"{type.AssemblyQualifiedName} is already registered")
        {
        }
    }
}