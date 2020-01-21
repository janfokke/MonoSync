using System;

namespace MonoSync.Exceptions
{
    public class IdentifierAlreadyRegisteredException : MonoSyncException
    {
        public IdentifierAlreadyRegisteredException(in int identifier, Type type) : base(
            $"Identifier: {identifier} is already registered for Type: {type.Name}")
        {
        }
    }
}