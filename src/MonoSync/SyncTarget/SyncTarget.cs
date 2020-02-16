using MonoSync.Utils;

namespace MonoSync
{
    public abstract class SyncTarget : SyncBase
    {
        public object BaseObject { get; protected set; }

        protected SyncTarget(int referenceId) : base(referenceId)
        {
        }

        public abstract void Read(ExtendedBinaryReader reader);
    }
}