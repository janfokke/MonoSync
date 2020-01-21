using System.Collections.Generic;

namespace MonoSync
{
    public abstract class SyncBase
    {
        protected SyncBase(int referenceId)
        {
            ReferenceId = referenceId;
        }

        public int ReferenceId { get; }
        public object BaseObject { get; protected set; }
        public abstract void Dispose();
        public abstract IEnumerable<object> GetReferences();
    }
}