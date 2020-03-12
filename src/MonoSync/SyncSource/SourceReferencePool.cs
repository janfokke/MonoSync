using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MonoSync.Exceptions;

namespace MonoSync
{
    internal class SourceReferencePool : IIdentifierResolver
    {
        private readonly ConditionalWeakTable<object, SyncSource> _syncObjects =
            new ConditionalWeakTable<object, SyncSource>();

        public IEnumerable<SyncSource> SyncObjects => _syncObjects.Select(x => x.Value);

        public int ResolveIdentifier(object reference)
        {
            if (reference == null)
            {
                return 0;
            }

            if (_syncObjects.TryGetValue(reference, out SyncSource value))
            {
                return value.ReferenceId;
            }

            throw new ReferenceIsNotTrackedException(reference);
        }

        public void AddSyncSource(SyncSource syncObject)
        {
            object baseObject = syncObject.Reference;
            _syncObjects.Add(baseObject, syncObject);
        }

        /// <summary>
        ///     Lookup the <see cref="TSync" /> of the <see cref="target" />
        /// </summary>
        /// <returns>The <see cref="TSync" /> if available. Else it returns null</returns>
        public SyncSource GetSyncSource(object reference)
        {
            if (_syncObjects.TryGetValue(reference, out SyncSource syncSource))
            {
                return syncSource;
            }

            return null;
        }

        /// <summary>Removes the reference.</summary>
        /// <param name="reference">The reference.</param>
        /// <returns>The referenceId of the reference that got removed. 0 if no reference was removed</returns>
        public void RemoveSyncSource(SyncSource syncSource)
        {
            _syncObjects.Remove(syncSource);
        }
    }
}