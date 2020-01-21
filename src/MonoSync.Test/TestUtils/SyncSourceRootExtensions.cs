using MonoSync.SyncSource;

namespace MonoSync.Test.TestUtils
{
    public static class SyncSourceRootExtensions
    {
        public static byte[] WriteFullAndDispose(this SyncSourceRoot syncSourceRoot)
        {
            using WriteSession session = syncSourceRoot.BeginWrite();
            return session.WriteFull();
        }

        public static SynchronizationPacket WriteChangesAndDispose(this SyncSourceRoot syncSourceRoot)
        {
            using WriteSession session = syncSourceRoot.BeginWrite();
            return session.WriteChanges();
        }
    }
}