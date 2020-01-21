using System;

namespace MonoSync.SyncSource
{
    public class WriteSession : IDisposable
    {
        private readonly SyncSourceRoot _syncSourceRoot;
        private bool _disposed;

        internal WriteSession(SyncSourceRoot syncSourceRoot)
        {
            _syncSourceRoot = syncSourceRoot;
        }

        public void Dispose()
        {
            CheckDisposed();
            _disposed = true;
            _syncSourceRoot.EndWrite();
        }

        public SynchronizationPacket WriteChanges()
        {
            CheckDisposed();
            return _syncSourceRoot.WriteChanges();
        }

        public byte[] WriteFull()
        {
            CheckDisposed();
            return _syncSourceRoot.WriteFull();
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(WriteSession));
            }
        }
    }
}