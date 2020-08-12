using System;

namespace MonoSync.Exceptions
{
    public class TypeNotSerializableException : MonoSyncException
    {
        public TypeNotSerializableException(string assemblyQualifiedName) : base($"Type {assemblyQualifiedName} is not serializable")
        {
        }
    }
}