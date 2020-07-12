namespace MonoSync.Test.TestUtils
{
    public static class SourceSynchronizerRootExtensions
    {
        public static byte[] WriteFullAndDispose(this SourceSynchronizerRoot SourceSynchronizerRoot)
        {
            using WriteSession session = SourceSynchronizerRoot.BeginWrite();
            return session.WriteFull();
        }

        public static SynchronizationPacket WriteChangesAndDispose(this SourceSynchronizerRoot SourceSynchronizerRoot)
        {
            using WriteSession session = SourceSynchronizerRoot.BeginWrite();
            return session.WriteChanges();
        }
    }
}