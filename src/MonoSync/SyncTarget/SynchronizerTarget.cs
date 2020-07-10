using MonoSync.Utils;

namespace MonoSync
{
    public abstract class SynchronizerTarget : SynchronizerBase
    {
        public object BaseObject { get; protected set; }

        protected SynchronizerTarget(int referenceId) : base(referenceId)
        {
        }

        public abstract void Read(ExtendedBinaryReader reader);
    }
}