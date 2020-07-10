using MonoSync.Utils;

namespace MonoSync.SyncSourceObjects
{
    public class StringSynchronizerSource : SynchronizerSource
    {
        public StringSynchronizerSource(SyncSourceRoot syncSourceRoot, int referenceId, string reference) :
            base(syncSourceRoot, referenceId, reference)
        {
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
            binaryWriter.Write((string) Reference);
        }
    }
}