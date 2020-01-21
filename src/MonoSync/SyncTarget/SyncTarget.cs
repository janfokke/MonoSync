using MonoSync.Utils;

namespace MonoSync.SyncTarget
{
    public abstract class SyncTarget : SyncBase
    {
        protected SyncTarget(int referenceId) : base(referenceId)
        {
        }

        public abstract void Read(ExtendedBinaryReader reader);
    }
}