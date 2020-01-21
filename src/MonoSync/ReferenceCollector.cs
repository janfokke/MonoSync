using System.Collections.Generic;

namespace MonoSync
{
    internal class ReferenceCollector<TSyncObject> where TSyncObject : SyncBase
    {
        private readonly ReferencePool<TSyncObject> _referencePool;

        public ReferenceCollector(ReferencePool<TSyncObject> referencePool)
        {
            _referencePool = referencePool;
        }

        /// <summary>
        ///     Recursively traverses child references of <see cref="root" />
        /// </summary>
        public HashSet<object> TraverseReferences(object root)
        {
            var output = new HashSet<object>();
            TraverseReferencesRecursive(root, output);
            return output;
        }

        private void TraverseReferencesRecursive(object reference, HashSet<object> output)
        {
            SyncBase syncBase = _referencePool.GetSyncObject(reference);

            if (syncBase == null)
            {
                return;
            }

            foreach (object childReference in syncBase.GetReferences())
            {
                if (childReference == null || output.Add(childReference) == false)
                {
                    continue;
                }

                TraverseReferencesRecursive(childReference, output);
            }
        }
    }
}