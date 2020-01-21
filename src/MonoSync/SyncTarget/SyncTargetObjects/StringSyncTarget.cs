using System.Collections.Generic;
using MonoSync.Utils;

namespace MonoSync.SyncTarget.SyncTargetObjects
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

        public override IEnumerable<object> GetReferences()
        {
            yield break;
        }

        public sealed override void Read(ExtendedBinaryReader reader)
        {
            // Ignore
        }
    }
}