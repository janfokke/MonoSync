using System;
using MonoSync.Utils;

namespace MonoSync.SyncSource
{
    public abstract class SyncSource : SyncBase, IDisposable
    {
        protected SyncSource(SyncSourceRoot syncSourceRoot, int referenceId, object baseObject) : base(
            referenceId)
        {
            SyncSourceRoot = syncSourceRoot;
            BaseObject = baseObject;
        }

        public int ReferenceCount { get; internal set; }

        public SyncSourceRoot SyncSourceRoot { get; }

        /// <summary>
        ///     This synchronization method is used to only synchronize the changes
        /// </summary>
        /// <param name="binaryWriter"></param>
        public abstract void WriteChanges(ExtendedBinaryWriter binaryWriter);

        /// <summary>
        ///     This synchronization method is used when an item is synchronized for the first time or when the synchronization
        ///     changes have become obsolete due to, for example, a reset of a collection.
        /// </summary>
        /// <param name="binaryWriter"></param>
        public abstract void WriteFull(ExtendedBinaryWriter binaryWriter);

        protected virtual void MarkDirty()
        {
            SyncSourceRoot.MarkDirty(this);
        }
    }
}