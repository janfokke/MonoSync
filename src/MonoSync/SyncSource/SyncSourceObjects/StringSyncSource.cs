using System.Collections.Generic;
using MonoSync.Utils;

namespace MonoSync.SyncSource.SyncSourceObjects
{
    public class StringSyncSource : SyncSource
    {
        public StringSyncSource(SyncSourceRoot syncSourceRoot, int referenceId, string baseObject) :
            base(syncSourceRoot, referenceId, baseObject)
        {
        }

        public override IEnumerable<object> GetReferences()
        {
            yield break;
        }

        public override void Dispose()
        {
            // Do Nothing
        }

        public override void WriteChanges(ExtendedBinaryWriter binaryWriter)
        {
            // Won't happen because strings are immutable
        }

        public override void WriteFull(ExtendedBinaryWriter binaryWriter)
        {
            binaryWriter.Write((string) BaseObject);
        }
    }
}