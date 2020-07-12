using MonoSync.Utils;

namespace MonoSync
{
    public abstract class TargetSynchronizer : SynchronizerBase
    {
        public object Reference { get; protected set; }

        protected TargetSynchronizer(int referenceId) : base(referenceId)
        {
        }

        public abstract void Read(ExtendedBinaryReader reader);
    }
}