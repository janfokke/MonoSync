using System;
using MonoSync.Utils;

namespace MonoSync
{
    public abstract class SynchronizerSource : SynchronizerBase
    {
        private bool _disposed;

        public SyncSourceRoot SyncSourceRoot { get; }

        /// <summary>
        ///     Reference to the target object
        /// </summary>
        public object Reference { get; }

        /// <summary>
        ///     Indicating whether a property has changed.
        /// </summary>
        public bool Dirty { get; private set; }

        protected SynchronizerSource(SyncSourceRoot syncSourceRoot, int referenceId, object reference) : base(referenceId)
        {
            SyncSourceRoot = syncSourceRoot;
            Reference = reference;
        }

        /// <summary>
        ///     Writes the changes to the <see cref="binaryWriter" />
        /// </summary>
        /// <param name="binaryWriter"></param>
        public abstract void WriteChanges(ExtendedBinaryWriter binaryWriter);

        /// <summary>
        ///     Writes the full object to the <see cref="binaryWriter" />.
        /// </summary>
        /// <param name="binaryWriter">The binary writer.</param>
        public abstract void WriteFull(ExtendedBinaryWriter binaryWriter);

        /// <summary>
        ///     Marks the target object dirty.
        /// </summary>
        protected virtual void MarkDirty()
        {
            Dirty = true;
            SyncSourceRoot.MarkSyncSourceDirty(this);
        }

        /// <summary>
        ///     Marks the target object clean.
        /// </summary>
        public virtual void MarkClean()
        {
            Dirty = false;
        }

        public override void Dispose()
        {
            DisposeImpl();
            GC.SuppressFinalize(this);
        }

        protected virtual void DisposeImpl()
        {
            if (_disposed)
            {
                return;
            }

            SyncSourceRoot.RegisterSyncSourceToBeUntracked(this);

            _disposed = true;
        }

        ~SynchronizerSource()
        {
            DisposeImpl();
        }
    }
}