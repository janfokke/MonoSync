using System;

namespace MonoSync.Exceptions
{
    internal class MultipleConstructorsException : MonoSyncException
    {
        public MultipleConstructorsException(Type baseType) : base($"Multiple constructors found in type: {baseType}.")
        {
            
        }
    }
}