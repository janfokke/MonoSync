using System;
using System.Collections.Generic;
using MonoSync.Exceptions;
using MonoSync.Utils;

namespace MonoSync
{
    internal class TargetReferencePool : IReferenceResolver
    {
        private readonly Dictionary<int, List<Action<object>>> _synchronizationCallbacks = new Dictionary<int, List<Action<object>>>();
        private readonly Dictionary<int, TargetSynchronizer> _syncObjectLookup = new Dictionary<int, TargetSynchronizer>();
        private readonly Dictionary<object, int> _syncObjectReferenceIdLookup = new Dictionary<object, int>(ReferenceEqualityComparer.Default);

        public IEnumerable<SynchronizerBase> SyncObjects => _syncObjectLookup.Values;

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
            if (TryGetSynchronizerByIdentifier(referenceId, out TargetSynchronizer syncObject))
            {
                synchronizationCallback(syncObject.Reference);
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

        public void AddSynchronizer(int referenceId, TargetSynchronizer targetSynchronizerObject)
        {
            object baseObject = targetSynchronizerObject.Reference;
            if (_syncObjectReferenceIdLookup.ContainsKey(baseObject))
                throw new InvalidOperationException("Synchronizer already added.");
            {
                _syncObjectReferenceIdLookup.Add(baseObject, referenceId);
                _syncObjectLookup.Add(referenceId, targetSynchronizerObject);
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
        public TargetSynchronizer GetSynchronizer(object reference)
        {
            if (_syncObjectReferenceIdLookup.TryGetValue(reference, out var referenceId))
            {
                if (_syncObjectLookup.TryGetValue(referenceId, out TargetSynchronizer syncObject))
                {
                    return syncObject;
                }
            }

            return null;
        }

        public bool TryGetSynchronizerByIdentifier(int referenceIdentifier, out TargetSynchronizer targetSynchronizerObject)
        {
            if (referenceIdentifier == 0)
            {
                targetSynchronizerObject = null;
                return true;
            }

            return _syncObjectLookup.TryGetValue(referenceIdentifier, out targetSynchronizerObject);
        }

        public void RemoveReference(int referenceId)
        {
            if (TryGetSynchronizerByIdentifier(referenceId, out TargetSynchronizer syncObject))
            {
                RemoveSynchronizer(syncObject);
            }
            else
            {
                throw new MonoSyncException("Cannot Remove untracked reference");
            }
        }

        public void RemoveSynchronizer(TargetSynchronizer targetSynchronizerObject)
        {
            targetSynchronizerObject.Dispose();
            _syncObjectLookup.Remove(targetSynchronizerObject.ReferenceId);
            _syncObjectReferenceIdLookup.Remove(targetSynchronizerObject.Reference);
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