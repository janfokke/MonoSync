using System;
using System.Collections.Generic;
using MonoSync.Exceptions;

namespace MonoSync
{
    internal class ReferencePool<TSync> :
        IReferenceResolver
        where TSync : SyncBase
    {
        private readonly Dictionary<int, List<Action<object>>> _referenceFixups =
            new Dictionary<int, List<Action<object>>>();

        private readonly Dictionary<int, TSync> _syncObjectLookup = new Dictionary<int, TSync>();
        private readonly Dictionary<object, int> _syncObjectReferenceIdLookup = new Dictionary<object, int>();

        public IEnumerable<TSync> SyncObjects => _syncObjectLookup.Values;

        /// <summary>
        ///     Resolves the reference if it is available or becomes available
        /// </summary>
        /// <remarks>Reference might never become available</remarks>
        /// <param name="referenceId"></param>
        /// <param name="fixup">When the reference is available this delegate will be called with the reference.</param>
        public void ResolveReference(int referenceId, Action<object> fixup)
        {
            if (referenceId == 0)
            {
                fixup(null);
                return;
            }

            // resolve immediately if reference is already resolved.
            if (TryGetSyncByIdentifier(referenceId, out TSync syncObject))
            {
                fixup(syncObject.BaseObject);
                return;
            }

            // Else register reference for fixup
            if (_referenceFixups.TryGetValue(referenceId, out List<Action<object>> referenceFixups))
            {
                referenceFixups.Add(fixup);
            }
            else
            {
                var newReferenceFixups = new List<Action<object>>();
                newReferenceFixups.Add(fixup);
                _referenceFixups.Add(referenceId, newReferenceFixups);
            }
        }

        public int ResolveIdentifier(object reference)
        {
            if (reference == null)
            {
                return 0;
            }

            return _syncObjectReferenceIdLookup[reference];
        }

        public void AddSyncObject(int referenceId, TSync syncObject)
        {
            object baseObject = syncObject.BaseObject;
            if (_syncObjectReferenceIdLookup.ContainsKey(baseObject) == false)
            {
                _syncObjectReferenceIdLookup.Add(baseObject, referenceId);
                _syncObjectLookup.Add(referenceId, syncObject);
                FixupReferenceDependencies(referenceId, baseObject);
            }
        }

        /// <summary>
        ///     Sometimes references are not synced and the fixups should be removed.
        ///     Example of how this can happen: if an Item is added to a Collection and later removed.
        ///     The initial add command will point to an empty reference.
        /// </summary>
        public void ClearFixups()
        {
            _referenceFixups.Clear();
        }

        private void FixupReferenceDependencies(int referenceId, object baseObject)
        {
            if (_referenceFixups.TryGetValue(referenceId, out List<Action<object>> fixups))
            {
                _referenceFixups.Remove(referenceId);
                foreach (Action<object> action in fixups)
                {
                    action(baseObject);
                }
            }
        }

        /// <summary>
        ///     Lookup the <see cref="TSync" /> of the <see cref="target" />
        /// </summary>
        /// <returns>The <see cref="TSync" /> if available. Else it returns null</returns>
        public TSync GetSyncObject(object reference)
        {
            if (_syncObjectReferenceIdLookup.TryGetValue(reference, out int referenceId))
            {
                if (_syncObjectLookup.TryGetValue(referenceId, out TSync syncObject))
                {
                    return syncObject;
                }
            }

            return null;
        }

        public bool ContainsReference(object target)
        {
            return _syncObjectReferenceIdLookup.ContainsKey(target);
        }

        public bool TryGetSyncByIdentifier(int referenceIdentifier, out TSync syncObject)
        {
            if (referenceIdentifier == 0)
            {
                syncObject = null;
                return true;
            }

            return _syncObjectLookup.TryGetValue(referenceIdentifier, out syncObject);
        }

        /// <summary>
        ///     Removes all references that do not occur in <see cref="occuringReferences" />
        /// </summary>
        /// <param name="occuringReferences"></param>
        public List<TSync> RemoveNonOccuringReferences(HashSet<object> occuringReferences)
        {
            var nonOccuringReferences = new List<object>();
            var removedSyncObjects = new List<TSync>();

            // find non-occuring references
            foreach (object reference in _syncObjectReferenceIdLookup.Keys)
            {
                if (occuringReferences.Contains(reference) == false)
                {
                    nonOccuringReferences.Add(reference);
                }
            }

            // remove non-occuring references
            for (var index = 0; index < nonOccuringReferences.Count; index++)
            {
                TSync syncObject = GetSyncObject(nonOccuringReferences[index]);
                RemoveSyncObject(syncObject);
                removedSyncObjects.Add(syncObject);
            }

            return removedSyncObjects;
        }

        /// <summary>Removes the reference.</summary>
        /// <param name="reference">The reference.</param>
        /// <returns>The referenceId of the reference that got removed. 0 if no reference was removed</returns>
        public int RemoveReference(object reference)
        {
            TSync syncObject = GetSyncObject(reference);
            if (syncObject == null)
            {
                return 0;
            }

            RemoveSyncObject(syncObject);
            return syncObject.ReferenceId;
        }

        public void RemoveReference(int referenceId)
        {
            if (TryGetSyncByIdentifier(referenceId, out TSync syncObject))
            {
                RemoveSyncObject(syncObject);
            }
            else
            {
                throw new MonoSyncException("Cannot Remove untracked reference");
            }
        }

        public void RemoveSyncObject(TSync syncObject)
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