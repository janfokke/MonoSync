using System;
using MonoSync.Exceptions;

namespace MonoSync.Synchronizers
{
    internal class MultipleConstructorsException : MonoSyncException
    {
        public MultipleConstructorsException(Type baseType) : base($"Multiple constructors found in type: {baseType}.")
        {
            
        }
    }
}