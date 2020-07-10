using System;

namespace MonoSync
{
    public abstract class SynchronizerBase : IDisposable
    {
        public int ReferenceId { get; }

        protected SynchronizerBase(int referenceId)
        {
            ReferenceId = referenceId;
        }

        public abstract void Dispose();
    }
}