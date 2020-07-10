﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonoSync.Exceptions;
using MonoSync.SyncSourceObjects;
using MonoSync.Utils;

namespace MonoSync
{
    public class SyncSourceRoot
    {
        private readonly HashSet<SynchronizerSource> _dirtySyncSourceObjects = new HashSet<SynchronizerSource>();
        private readonly SerializerCollection _serializers;

        /// <summary>
        ///     If a target to an object is added and the object references itself during the construction
        ///     it causes an infinite loop because the object will never be added to the <see cref="SourceReferencePool" />.
        /// </summary>
        private readonly HashSet<object> _pendingForCreation = new HashSet<object>();
        private readonly HashSet<SynchronizerSource> _pendingTrackedSyncSourceObjects = new HashSet<SynchronizerSource>();
        private readonly HashSet<SynchronizerSource> _pendingUntrackedSyncSourceObjects = new HashSet<SynchronizerSource>();

        private readonly SourceReferencePool _referencePool = new SourceReferencePool();
        private readonly PropertyCollection.Factory _syncPropertyFactory;
        private int _referenceIdIncrementer = 1; //Reference index 0 is reserved for null
        private bool _writeSessionOpen;
        private readonly TypeEncoder _typeEncoder;
        private SynchronizerCollection _sourceFactoryResolver;

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
        public IEnumerable<object> DirtyObjects =>
            _dirtySyncSourceObjects.Select(sourceObject => sourceObject.Reference);

        public SyncSourceRoot(object source)
        {
            _typeEncoder = new TypeEncoder(new TypeTable());
            _sourceFactoryResolver = new SynchronizerCollection();

            _serializers = new SerializerCollection(_referencePool);
            _syncPropertyFactory = new PropertyCollection.Factory(_serializers);
            TrackObject(source);
        }

        /// <summary>
        ///     Gets the type of the properties from.
        /// </summary>
        /// <param name="baseType">Type of the base.</param>
        internal PropertyCollection GetPropertiesFromType(Type baseType)
        {
            return _syncPropertyFactory.FromType(baseType);
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

            _dirtySyncSourceObjects.Clear();
            _pendingTrackedSyncSourceObjects.Clear();
            _pendingUntrackedSyncSourceObjects.Clear();
        }

        /// <summary>
        ///     Used for existing connections to serializes changed and added references
        /// </summary>
        /// <returns></returns>
        internal SynchronizationPacket WriteChanges()
        {
            var memoryStream = new MemoryStream();
            var writer = new ExtendedBinaryWriter(memoryStream);

            WriteUntrackedReferences(writer);

            WriteAddedAndChangedReferences(writer);

            return new SynchronizationPacket(memoryStream.ToArray());
        }

        private void WriteUntrackedReferences(ExtendedBinaryWriter writer)
        {
            lock (_pendingUntrackedSyncSourceObjects)
            {
                writer.Write7BitEncodedInt(_pendingUntrackedSyncSourceObjects.Count);
                foreach (SynchronizerSource syncSource in _pendingUntrackedSyncSourceObjects)
                {
                    writer.Write7BitEncodedInt(syncSource.ReferenceId);
                }
            }
        }

        private void WriteAddedAndChangedReferences(ExtendedBinaryWriter writer)
        {
            List<SynchronizerSource> changedSyncSourceObjects = _dirtySyncSourceObjects.ToList();
            var changedAndNewReferenceUnion = new HashSet<SynchronizerSource>(_pendingTrackedSyncSourceObjects);
            // Merge the new references and the added ones
            // Because it is possible for references to be both added and changed as wel
            changedAndNewReferenceUnion.UnionWith(changedSyncSourceObjects);

            var referenceCount = changedAndNewReferenceUnion.Count;

            writer.Write7BitEncodedInt(referenceCount);

            foreach (SynchronizerSource syncSourceObject in changedAndNewReferenceUnion)
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

        internal byte[] WriteFull()
        {
            using var memoryStream = new MemoryStream();
            using var writer = new ExtendedBinaryWriter(memoryStream);

            // Write remove target count 0
            writer.Write7BitEncodedInt(0);

            List<SynchronizerSource> syncSourceObjects = _referencePool.SyncObjects.ToList();
            writer.Write7BitEncodedInt(syncSourceObjects.Count);
            foreach (SynchronizerSource syncSourceObject in syncSourceObjects)
            {
                writer.Write7BitEncodedInt(syncSourceObject.ReferenceId);
                _typeEncoder.WriteType(syncSourceObject.Reference.GetType(), writer);
                syncSourceObject.WriteFull(writer);
            }

            return new SynchronizationPacket(memoryStream.ToArray()).SetTick(0);
        }

        /// <summary>
        ///     Registers the <see cref="syncSource" /> for removal.
        ///     the <see cref="syncSource" /> can be revived by calling <see cref="TrackObject" />
        ///     on the object that the <see cref="syncSource" /> references.
        /// </summary>
        /// <param name="synchronizerSourcenchronize source.</param>
        /// <exception cref="ArgumentNullException">synchronizerSource</exception>
        internal void RegisterSyncSourceToBeUntracked(SynchronizerSource synchronizerSource)
        {
            lock (_pendingUntrackedSyncSourceObjects)
            {
                if (synchronizerSource == null)
                {
                    throw new ArgumentNullException(nameof(synchronizerSource));
                }
                _pendingUntrackedSyncSourceObjects.Add(synchronizerSource);
            }
        }

        /// <summary>
        ///     Tracks the <see cref="target" /> for synchronization
        /// </summary>
        /// <param name="target">The target.</param>
        /// <remarks>The <see cref="target">target's</see> synchronize-able child references are also tracked recursively</remarks>
        /// <exception cref="ArgumentNullException">target</exception>
        public void TrackObject(object target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            SynchronizerSource synchronizerSource = _referencePool.GetSyncSource(target);

            if (synchronizerSource == null)
            {
                if (_pendingForCreation.Contains(target))
                {
                    return;
                }

                ISyncSourceFactory sourceFactory = _sourceFactoryResolver.FindMatchingSyncSourceFactory(target);
                var referenceId = _referenceIdIncrementer++;
                _pendingForCreation.Add(target);
                synchronizerSource = sourceFactory.Create(this, referenceId, target);
                _pendingForCreation.Remove(target);
                _referencePool.AddSyncSource(synchronizerSource);
                _pendingTrackedSyncSourceObjects.Add(synchronizerSource);
            }
        }

        /// <summary>
        ///     Indicates that the <see cref="syncSource" /> has changes and should be serialized on next
        ///     <see cref="WriteChanges" />.
        /// </summary>
        /// <param name="synchronizerSourcenchronize source.</param>
        public void MarkSyncSourceDirty(SynchronizerSource synchronizerSource)
        {
            _dirtySyncSourceObjects.Add(synchronizerSource);
        }
    }
}