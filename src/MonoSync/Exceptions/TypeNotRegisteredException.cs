using System;

namespace MonoSync.Exceptions
{
    public class TypeNotRegisteredException : MonoSyncException
    {
        public TypeNotRegisteredException(Type type) : base($"Type {type.AssemblyQualifiedName} is not registered")
        {
        }
    }
}