using MonoSync.SyncSourceObjects;

namespace MonoSync.SyncSourceObjectFactorties
{
    internal class StringSyncSourceFactory : ISyncSourceFactory
    {
        public bool CanCreate(object baseType)
        {
            return baseType is string;
        }

        public SynchronizerSource Create(SyncSourceRoot syncSourceRoot, int referenceId, object baseType)
        {
            return new StringSynchronizerSource(syncSourceRoot, referenceId, baseType as string);
        }
    }
}