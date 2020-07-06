using System;
using System.Collections.Generic;
using MonoSync.Exceptions;
using MonoSync.Utils;

namespace MonoSync
{
    internal class TargetReferencePool : IReferenceResolver
    {
        private readonly Dictionary<int, List<Action<object>>> _synchronizationCallbacks =
            new Dictionary<int, List<Action<object>>>();
        private readonly Dictionary<int, SyncTarget> _syncObjectLookup = new Dictionary<int, SyncTarget>();
        private readonly Dictionary<object, int> _syncObjectReferenceIdLookup = new Dictionary<object, int>(ReferenceEqualityComparer.Default);

        public IEnumerable<SyncBase> SyncObjects => _syncObjectLookup.Values;

        /// <summary>
        ///     Resolves the reference if it is available or becomes available
        /// </summary>
        /// <remarks>Reference might never become available</remarks>
        /// <param name="referenceId"></param>
        /// <param name="synchronizationCallback">When the reference is available this delegate will be called with the reference.</param>
        public void ResolveReference(in int referenceId, Action<object> synchronizationCallback)
        {
            if (referenceId == 0)
            {
                synchronizationCallback(null);
                return;
            }

            // resolve immediately if reference is already resolved.
            if (TryGetSyncTargetByIdentifier(referenceId, out SyncTarget syncObject))
            {
                synchronizationCallback(syncObject.BaseObject);
                return;
            }

            // Else register reference for synchronizationCallback
            if (_synchronizationCallbacks.TryGetValue(referenceId, out List<Action<object>> synchronizationCallbacks))
            {
                synchronizationCallbacks.Add(synchronizationCallback);
            }
            else
            {
                _synchronizationCallbacks.Add(referenceId, new List<Action<object>>
                {
                    synchronizationCallback
                });
            }
        }

        public void AddSyncObject(int referenceId, SyncTarget syncObject)
        {
            object baseObject = syncObject.BaseObject;
            if (_syncObjectReferenceIdLookup.ContainsKey(baseObject))
                throw new DoubleSynchronizedReferenceException();
            {
                _syncObjectReferenceIdLookup.Add(baseObject, referenceId);
                _syncObjectLookup.Add(referenceId, syncObject);
                ResolveReferenceDependencies(referenceId, baseObject);
            }
        }

        private void ResolveReferenceDependencies(int referenceId, object reference)
        {
            if (_synchronizationCallbacks.TryGetValue(referenceId, out List<Action<object>> synchronizationCallback))
            {
                _synchronizationCallbacks.Remove(referenceId);
                foreach (Action<object> action in synchronizationCallback)
                {
                    action(reference);
                }
            }
        }

        /// <summary>
        ///     Lookup the <see cref="TSync" /> of the <see cref="target" />
        /// </summary>
        /// <returns>The <see cref="TSync" /> if available. Else it returns null</returns>
        public SyncTarget GetSyncObject(object reference)
        {
            if (_syncObjectReferenceIdLookup.TryGetValue(reference, out var referenceId))
            {
                if (_syncObjectLookup.TryGetValue(referenceId, out SyncTarget syncObject))
                {
                    return syncObject;
                }
            }

            return null;
        }

        public bool TryGetSyncTargetByIdentifier(int referenceIdentifier, out SyncTarget syncObject)
        {
            if (referenceIdentifier == 0)
            {
                syncObject = null;
                return true;
            }

            return _syncObjectLookup.TryGetValue(referenceIdentifier, out syncObject);
        }

        public void RemoveReference(int referenceId)
        {
            if (TryGetSyncTargetByIdentifier(referenceId, out SyncTarget syncObject))
            {
                RemoveSyncObject(syncObject);
            }
            else
            {
                throw new MonoSyncException("Cannot Remove untracked reference");
            }
        }

        public void RemoveSyncObject(SyncTarget syncObject)
        {
            syncObject.Dispose();
            _syncObjectLookup.Remove(syncObject.ReferenceId);
            _syncObjectReferenceIdLookup.Remove(syncObject.BaseObject);
        }

        public void RemoveReferences(int[] removedReferenceIds)
        {
            for (var i = 0; i < removedReferenceIds.Length; i++)
            {
                RemoveReference(removedReferenceIds[i]);
            }
        }
    }
}