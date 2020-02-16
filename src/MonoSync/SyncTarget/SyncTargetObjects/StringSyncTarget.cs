using MonoSync.Utils;

namespace MonoSync.SyncTargetObjects
{
    public class StringSyncTarget : SyncTarget
    {
        public StringSyncTarget(int referenceId, ExtendedBinaryReader reader) : base(referenceId)
        {
            BaseObject = reader.ReadString();
        }

        public override void Dispose()
        {
            // Ignore
        }

        public sealed override void Read(ExtendedBinaryReader reader)
        {
            // Ignore
        }
    }
}