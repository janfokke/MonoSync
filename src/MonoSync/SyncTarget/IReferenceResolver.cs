using System;

namespace MonoSync
{
    public interface IReferenceResolver
    {
        void ResolveReference(in int referenceId, Action<object> synchronizationCallback);
    }
}