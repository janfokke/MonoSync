using System;
using System.Collections.Generic;
using MonoSync.Exceptions;

namespace MonoSync
{
    internal class TargetReferencePool : IReferenceResolver
    {
        private readonly Dictionary<int, List<Action<object>>> _referenceFixups =
            new Dictionary<int, List<Action<object>>>();
        private readonly Dictionary<int, SyncTarget> _syncObjectLookup = new Dictionary<int, SyncTarget>();
        private readonly Dictionary<object, int> _syncObjectReferenceIdLookup = new Dictionary<object, int>();

        public IEnumerable<SyncBase> SyncObjects => _syncObjectLookup.Values;

        /// <summary>
        ///     Resolves the reference if it is available or becomes available
        /// </summary>
        /// <remarks>Reference might never become available</remarks>
        /// <param name="referenceId"></param>
        /// <param name="fixup">When the reference is available this delegate will be called with the reference.</param>
        public void ResolveReference(in int referenceId, Action<object> fixup)
        {
            if (referenceId == 0)
            {
                fixup(null);
                return;
            }

            // resolve immediately if reference is already resolved.
            if (TryGetSyncTargetByIdentifier(referenceId, out SyncTarget syncObject))
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

        public void AddSyncObject(int referenceId, SyncTarget syncObject)
        {
            object baseObject = syncObject.BaseObject;
            if (_syncObjectReferenceIdLookup.ContainsKey(baseObject) == false)
            {
                _syncObjectReferenceIdLookup.Add(baseObject, referenceId);
                _syncObjectLookup.Add(referenceId, syncObject);
                FixupReferenceDependencies(referenceId, baseObject);
            }
        }

        private void FixupReferenceDependencies(int referenceId, object reference)
        {
            if (_referenceFixups.TryGetValue(referenceId, out List<Action<object>> fixups))
            {
                _referenceFixups.Remove(referenceId);
                foreach (Action<object> action in fixups)
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