using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using MonoSync.Exceptions;
using MonoSync.Serializers;
using MonoSync.Synchronizers;
using MonoSync.Utils;

namespace MonoSync
{
    public class SourceSynchronizerRoot
    {
        private readonly HashSet<SourceSynchronizer> _dirtySyncSourceObjects = new HashSet<SourceSynchronizer>();

        /// <summary>
        ///     If a reference to an object is added and the object references itself during the construction
        ///     it causes an infinite loop because the object will never be added to the <see cref="SourceReferencePool" />.
        /// </summary>
        private readonly HashSet<object> _pendingForSynchronization = new HashSet<object>();
        private readonly HashSet<SourceSynchronizer> _pendingTrackedSyncSourceObjects = new HashSet<SourceSynchronizer>();
        private readonly HashSet<SourceSynchronizer> _pendingUntrackedSyncSourceObjects = new HashSet<SourceSynchronizer>();

        private readonly SourceReferencePool _referencePool = new SourceReferencePool();
        
        private int _referenceIdIncrementer = 1; //Reference index 0 is reserved for null
        private bool _writeSessionOpen;
        private readonly TypeEncoder _typeEncoder = new TypeEncoder();
        
        internal SynchronizableMemberFactory SynchronizableMemberFactory { get; } 

        /// <summary>
        ///     Gets the objects that are being tracked.
        /// </summary>
        public IEnumerable<object> TrackedObjects =>
            _referencePool.SyncObjects.Select(sourceObject => sourceObject.Reference);

        /// <summary>
        ///     Gets the objects that have been tracked since the previous write session
        /// </summary>
        public IEnumerable<object> PendingTrackedObjects =>
            _pendingTrackedSyncSourceObjects.Select(sourceObject => sourceObject.Reference);


        /// <summary>Gets the pending untracked object count.</summary>
        public int PendingUntrackedObjectCount => _pendingUntrackedSyncSourceObjects.Count;

        /// <summary>
        ///     Objects that have changed since the previous write session
        /// </summary>
        public IEnumerable<object> DirtyObjects => _dirtySyncSourceObjects.Select(sourceObject => sourceObject.Reference);
        public Settings Settings { get; }

        public SourceSynchronizerRoot(object reference, Settings settings = null)
        {
            Settings = settings??Settings.Default();
            Settings.Serializers.AddSerializer(new SourceReferenceSerializer(_referencePool));
            SynchronizableMemberFactory = new SynchronizableMemberFactory(Settings.Serializers);
            Synchronize(reference);
        }

        /// <summary>
        ///     Begins a write session. Call <see cref="WriteSession.Dispose" /> to end the write session
        /// </summary>
        /// <exception cref="WriteSessionNotClosedException"></exception>
        public WriteSession BeginWrite()
        {
            if (_writeSessionOpen)
            {
                throw new WriteSessionNotClosedException();
            }

            _writeSessionOpen = true;

            return new WriteSession(this);
        }

        /// <summary>
        ///     Ends the write session.
        /// </summary>
        /// <exception cref="WriteSessionNotOpenException"></exception>
        internal void EndWrite()
        {
            if (_writeSessionOpen == false)
            {
                throw new WriteSessionNotOpenException();
            }

            _writeSessionOpen = false;
            _typeEncoder.EndWrite();
            _dirtySyncSourceObjects.Clear();
            _pendingTrackedSyncSourceObjects.Clear();
            _pendingUntrackedSyncSourceObjects.Clear();
        }

        internal byte[] WriteFull()
        {
            using var memoryStream = new MemoryStream();
            using var writer = new ExtendedBinaryWriter(memoryStream);

            _typeEncoder.WriteAllTypes(writer);

            // Write remove reference count 0
            writer.Write7BitEncodedInt(0);

            List<SourceSynchronizer> syncSourceObjects = _referencePool.SyncObjects.ToList();
            writer.Write7BitEncodedInt(syncSourceObjects.Count);
            foreach (SourceSynchronizer syncSourceObject in syncSourceObjects)
            {
                writer.Write7BitEncodedInt(syncSourceObject.ReferenceId);
                _typeEncoder.WriteType(syncSourceObject.Reference.GetType(), writer);
                syncSourceObject.WriteFull(writer);
            }
            return new SynchronizationPacket(memoryStream.ToArray()).SetTick(TimeSpan.Zero);
        }

        /// <summary>
        ///     Used for existing connections to serializes changed and added references
        /// </summary>
        /// <returns></returns>
        internal SynchronizationPacket WriteChanges()
        {
            var memoryStream = new MemoryStream();
            var writer = new ExtendedBinaryWriter(memoryStream);

            _typeEncoder.WriteAddedTypes(writer);

            WriteUntrackedReferences(writer);

            WriteAddedAndChangedReferences(writer);

            return new SynchronizationPacket(memoryStream.ToArray());
        }

        private void WriteUntrackedReferences(ExtendedBinaryWriter writer)
        {
            lock (_pendingUntrackedSyncSourceObjects)
            {
                writer.Write7BitEncodedInt(_pendingUntrackedSyncSourceObjects.Count);
                foreach (SourceSynchronizer syncSource in _pendingUntrackedSyncSourceObjects)
                {
                    writer.Write7BitEncodedInt(syncSource.ReferenceId);
                }
            }
        }

        private void WriteAddedAndChangedReferences(ExtendedBinaryWriter writer)
        {
            List<SourceSynchronizer> changedSyncSourceObjects = _dirtySyncSourceObjects.ToList();
            var changedAndNewReferenceUnion = new HashSet<SourceSynchronizer>(_pendingTrackedSyncSourceObjects);
            // Merge the new references and the added ones
            // Because it is possible for references to be both added and changed as wel
            changedAndNewReferenceUnion.UnionWith(changedSyncSourceObjects);

            var referenceCount = changedAndNewReferenceUnion.Count;

            writer.Write7BitEncodedInt(referenceCount);

            foreach (SourceSynchronizer syncSourceObject in changedAndNewReferenceUnion)
            {
                writer.Write7BitEncodedInt(syncSourceObject.ReferenceId);

                if (_pendingTrackedSyncSourceObjects.Contains(syncSourceObject))
                {
                    _typeEncoder.WriteType(syncSourceObject.Reference.GetType(), writer);
                    syncSourceObject.WriteFull(writer);
                }
                else
                {
                    syncSourceObject.WriteChanges(writer);
                }
            }
        }

        /// <summary>
        ///     Registers the <see cref="syncSource" /> for removal.
        ///     the <see cref="syncSource" /> can be revived by calling <see cref="Synchronize" />
        ///     on the object that the <see cref="syncSource" /> references.
        /// </summary>
        /// <param name="synchronizerSourcenchronize reference.</param>
        /// <exception cref="ArgumentNullException">sourceSynchronizer</exception>
        internal void RegisterSyncSourceToBeUntracked(SourceSynchronizer sourceSynchronizer)
        {
            lock (_pendingUntrackedSyncSourceObjects)
            {
                if (sourceSynchronizer == null)
                {
                    throw new ArgumentNullException(nameof(sourceSynchronizer));
                }
                _pendingUntrackedSyncSourceObjects.Add(sourceSynchronizer);
            }
        }

        /// <summary>
        ///     Tracks the <see cref="target" /> for synchronization
        /// </summary>
        /// <param name="referencehe reference.</param>
        /// <remarks>The <see cref="target">reference's</see> synchronize-able child references are also tracked recursively</remarks>
        /// <exception cref="ArgumentNullException">reference</exception>
        public void Synchronize(object reference)
        {
            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            SourceSynchronizer sourceSynchronizer = _referencePool.GetSyncSource(reference);

            if (sourceSynchronizer == null)
            {
                if (_pendingForSynchronization.Contains(reference))
                {
                    return;
                }

                Type referenceType = reference.GetType();
                if(referenceType.HasElementType)
                {
                    _typeEncoder.RegisterType(referenceType.GetElementType());
                }
                _typeEncoder.RegisterType(referenceType);

                ISynchronizer sourceFactory = Settings.Synchronizers.FindSynchronizerByType(referenceType);
                var referenceId = _referenceIdIncrementer++;
                _pendingForSynchronization.Add(reference);
                sourceSynchronizer = sourceFactory.Synchronize(this, referenceId, reference);
                _pendingForSynchronization.Remove(reference);
                _referencePool.AddSyncSource(sourceSynchronizer);
                _pendingTrackedSyncSourceObjects.Add(sourceSynchronizer);
            }
        }

        /// <summary>
        ///     Indicates that the <see cref="syncSource" /> has changes and should be serialized on next
        ///     <see cref="WriteChanges" />.
        /// </summary>
        /// <param name="synchronizerSourcenchronize reference.</param>
        public void MarkDirty(SourceSynchronizer sourceSynchronizer)
        {
            _dirtySyncSourceObjects.Add(sourceSynchronizer);
        }
    }
}