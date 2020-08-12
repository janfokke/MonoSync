using MonoSync.Utils;

namespace MonoSync.Synchronizers
{
    public class StringSourceSynchronizer : SourceSynchronizer
    {
        public StringSourceSynchronizer(SourceSynchronizerRoot sourceSynchronizerRoot, int referenceId, string reference) :
            base(sourceSynchronizerRoot, referenceId, reference)
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