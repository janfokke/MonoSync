using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonoSync.Exceptions;
using MonoSync.Utils;

namespace MonoSync.SyncSource
{
    public class SyncSourceRoot
    {
        private readonly HashSet<SyncSource> _addedSyncSourceObjects = new HashSet<SyncSource>();
        private readonly HashSet<SyncSource> _dirtySyncSourceObjects = new HashSet<SyncSource>();
        private readonly IFieldSerializerResolver _fieldDeserializerResolver;

        // Used to avoid endless loop
        private readonly Dictionary<object, int> _pendingForCreationReferenceCount = new Dictionary<object, int>();
        private readonly ReferenceCollector<SyncSource> _referenceCollector;
        private readonly ReferencePool<SyncSource> _referencePool = new ReferencePool<SyncSource>();
        private readonly HashSet<SyncSource> _removedSyncSourceObjects = new HashSet<SyncSource>();
        private readonly object _rootReference;
        private int _referenceIdIncrementer = 1; //Reference index 0 is reserved for null
        private bool _writeSessionOpen;

        public SyncSourceRoot(object source, SyncSourceSettings settings)
        {
            Settings = settings;
            _fieldDeserializerResolver = settings.FieldDeserializerResolverFactory.Create(_referencePool);
            _referenceCollector = new ReferenceCollector<SyncSource>(_referencePool);
            AddReference(_rootReference = source);
        }

        public IEnumerable<object> AddedReferences =>
            _addedSyncSourceObjects.Select(sourceObject => sourceObject.BaseObject);

        public IEnumerable<object> RemovedReferences =>
            _removedSyncSourceObjects.Select(sourceObject => sourceObject.BaseObject);

        public IEnumerable<object> DirtyReferences =>
            _dirtySyncSourceObjects.Select(sourceObject => sourceObject.BaseObject);

        public IEnumerable<object> TrackedReferences =>
            _referencePool.SyncObjects.Select(sourceObject => sourceObject.BaseObject);

        public SyncSourceSettings Settings { get; }

        public WriteSession BeginWrite()
        {
            if (_writeSessionOpen) throw new WriteSessionNotClosedException();

            _writeSessionOpen = true;

            return new WriteSession(this);
        }

        /// <summary>
        ///     Used for existing connections to serializes changed and added references
        /// </summary>
        /// <returns></returns>
        internal SynchronizationPacket WriteChanges()
        {
            var memoryStream = new MemoryStream();
            var writer = new ExtendedBinaryWriter(memoryStream);

            WriteRemovedReferences(writer);

            WriteAddedAndChangedReferences(writer);

            return new SynchronizationPacket(memoryStream.ToArray());
        }

        /// <summary>
        ///     Removes all SyncSourceObjects from reference pool
        /// </summary>
        private void RemoveSyncSourceObjects()
        {
            foreach (SyncSource removedSyncSourceObject in _removedSyncSourceObjects)
                RemoveSyncSourceObject(removedSyncSourceObject);
            _removedSyncSourceObjects.Clear();
        }

        /// <summary>
        ///     Removes the SyncSourceObject from referencePool.
        /// </summary>
        /// <param name="removedSyncSourceObject">The removed synchronize source object.</param>
        private void RemoveSyncSourceObject(SyncSource removedSyncSourceObject)
        {
            _referencePool.RemoveSyncObject(removedSyncSourceObject);
        }

        private void WriteRemovedReferences(ExtendedBinaryWriter writer)
        {
            writer.Write7BitEncodedInt(_removedSyncSourceObjects.Count);
            foreach (SyncSource removedSyncSourceObject in _removedSyncSourceObjects)
                writer.Write7BitEncodedInt(removedSyncSourceObject.ReferenceId);
        }

        private void WriteAddedAndChangedReferences(ExtendedBinaryWriter writer)
        {
            List<SyncSource> changedSyncSourceObjects = _dirtySyncSourceObjects.ToList();
            var changedAndNewReferenceUnion = new HashSet<SyncSource>(_addedSyncSourceObjects);
            // Merge the new references and the added ones
            // Because it is possible for references to be both added and changed as wel
            changedAndNewReferenceUnion.UnionWith(changedSyncSourceObjects);

            var referenceCount = changedAndNewReferenceUnion.Count;

            writer.Write7BitEncodedInt(referenceCount);

            foreach (SyncSource syncSourceObject in changedAndNewReferenceUnion)
            {
                writer.Write7BitEncodedInt(syncSourceObject.ReferenceId);

                if (_addedSyncSourceObjects.Contains(syncSourceObject))
                {
                    Settings.TypeEncoder.WriteType(syncSourceObject.BaseObject.GetType(), writer);
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

            // Write remove reference count 0
            writer.Write7BitEncodedInt(0);

            List<SyncSource> syncSourceObjects = _referencePool.SyncObjects.ToList();
            writer.Write7BitEncodedInt(syncSourceObjects.Count);
            foreach (SyncSource syncSourceObject in syncSourceObjects)
            {
                writer.Write7BitEncodedInt(syncSourceObject.ReferenceId);
                Settings.TypeEncoder.WriteType(syncSourceObject.BaseObject.GetType(), writer);
                syncSourceObject.WriteFull(writer);
            }

            return new SynchronizationPacket(memoryStream.ToArray()).SetTick(0);
        }

        internal void EndWrite()
        {
            if (_writeSessionOpen == false) throw new WriteSessionNotOpenException();

            _writeSessionOpen = false;

            RemoveSyncSourceObjects();

            _dirtySyncSourceObjects.Clear();
            _addedSyncSourceObjects.Clear();
        }

        public void RemoveReference(object reference)
        {
            if (reference == null) throw new ArgumentNullException(nameof(reference));

            SyncSource syncObject = _referencePool.GetSyncObject(reference);
            if (syncObject == null) throw new InvalidOperationException($"{nameof(reference)} is not tracked");

            if (--syncObject.ReferenceCount <= 0)
            {
                var isAlreadySynchronized = _addedSyncSourceObjects.Remove(syncObject) == false;
                if (isAlreadySynchronized)
                    // Clients do not need to be notified of deleted objects
                    // that have not yet been synchronized.
                    _removedSyncSourceObjects.Add(syncObject);
                // If reference is not tracked by targets remove immediately
                else
                    RemoveSyncSourceObject(syncObject);
            }
        }

        public void AddReference(object reference)
        {
            if (reference == null) throw new ArgumentNullException(nameof(reference));

            if (_pendingForCreationReferenceCount.ContainsKey(reference))
            {
                _pendingForCreationReferenceCount[reference]++;
                return;
            }

            SyncSource syncSource = _referencePool.GetSyncObject(reference);
            if (syncSource == null)
            {
                ISyncSourceFactory sourceFactory = Settings.SyncSourceFactoryResolver
                    .FindMatchingSyncSourceFactory(reference);
                var referenceId = _referenceIdIncrementer++;

                _pendingForCreationReferenceCount[reference] = 1;
                syncSource =
                    sourceFactory.Create(this, referenceId, reference,
                        _fieldDeserializerResolver);
                syncSource.ReferenceCount = _pendingForCreationReferenceCount[reference];
                _pendingForCreationReferenceCount.Remove(reference);

                _referencePool.AddSyncObject(referenceId, syncSource);
                _addedSyncSourceObjects.Add(syncSource);
            }
            else
            {
                _removedSyncSourceObjects.Remove(syncSource);
                syncSource.ReferenceCount++;
            }
        }

        public void GarbageCollect()
        {
            HashSet<object> referenceThatAreAccessibleFromRoot = TraverseReferences();

            List<SyncSource> removeSyncSources =
                _referencePool.GetNonOccuringReferences(referenceThatAreAccessibleFromRoot);

            foreach (SyncSource removeSyncSource in removeSyncSources) RemoveReference(removeSyncSource.BaseObject);
        }

        private HashSet<object> TraverseReferences()
        {
            void TraverseReferencesRecursive(object reference, HashSet<object> output)
            {
                HashSet<object> references = _referenceCollector.TraverseReferences(reference);

                // Add the new references to the referencePool
                foreach (object occuringReference in references)
                    // Check if referencePool contains reference if it is not yet added to output
                    if (output.Add(occuringReference))
                        if (_referencePool.ContainsReference(occuringReference) == false)
                            // This error occurs when a SyncSource doesn't track a reference it should have tracked
                            throw new UntrackedReferenceException(reference, occuringReference);
            }

            var occuringReferences = new HashSet<object> {_rootReference};
            TraverseReferencesRecursive(_rootReference, occuringReferences);
            return occuringReferences;
        }


        public void MarkDirty(SyncSource syncSource)
        {
            _dirtySyncSourceObjects.Add(syncSource);
        }
    }
}