using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MonoSync.Exceptions;

namespace MonoSync
{
    internal class SourceReferencePool : IIdentifierResolver
    {
        private readonly ConditionalWeakTable<object, SynchronizerSource> _syncObjects =
            new ConditionalWeakTable<object, SynchronizerSource>();

        public IEnumerable<SynchronizerSource> SyncObjects => _syncObjects.Select(x => x.Value);

        public int ResolveIdentifier(object reference)
        {
            if (reference == null)
            {
                return 0;
            }

            if (_syncObjects.TryGetValue(reference, out SynchronizerSource value))
            {
                return value.ReferenceId;
            }

            throw new ReferenceIsNotTrackedException(reference);
        }

        public void AddSyncSource(SynchronizerSource synchronizerObject)
        {
            object baseObject = synchronizerObject.Reference;
            _syncObjects.Add(baseObject, synchronizerObject);
        }

        /// <summary>
        ///     Lookup the <see cref="TSync" /> of the <see cref="target" />
        /// </summary>
        /// <returns>The <see cref="TSync" /> if available. Else it returns null</returns>
        public SynchronizerSource GetSyncSource(object reference)
        {
            if (_syncObjects.TryGetValue(reference, out SynchronizerSource syncSource))
            {
                return syncSource;
            }

            return null;
        }

        /// <summary>Removes the reference.</summary>
        /// <param name="reference">The reference.</param>
        /// <returns>The referenceId of the reference that got removed. 0 if no reference was removed</returns>
        public void RemoveSyncSource(SynchronizerSource synchronizerSource)
        {
            _syncObjects.Remove(synchronizerSource);
        }
    }
}