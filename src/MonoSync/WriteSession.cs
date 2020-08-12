using System;

namespace MonoSync
{
    public class WriteSession : IDisposable
    {
        private readonly SourceSynchronizerRoot _sourceSynchronizerRoot;
        private bool _disposed;

        internal WriteSession(SourceSynchronizerRoot sourceSynchronizerRoot)
        {
            _sourceSynchronizerRoot = sourceSynchronizerRoot;
        }

        public void Dispose()
        {
            CheckDisposed();
            _disposed = true;
            _sourceSynchronizerRoot.EndWrite();
        }

        public SynchronizationPacket WriteChanges()
        {
            CheckDisposed();
            return _sourceSynchronizerRoot.WriteChanges();
        }

        public byte[] WriteFull()
        {
            CheckDisposed();
            return _sourceSynchronizerRoot.WriteFull();
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