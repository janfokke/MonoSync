using System;

namespace MonoSync
{
    public abstract class SyncBase : IDisposable
    {
        public int ReferenceId { get; }

        protected SyncBase(int referenceId)
        {
            ReferenceId = referenceId;
        }

        public abstract void Dispose();
    }
}