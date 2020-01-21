using System;

namespace MonoSync
{
    public interface IReferenceResolver
    {
        int ResolveIdentifier(object reference);
        void ResolveReference(int referenceId, Action<object> fixup);
    }
}