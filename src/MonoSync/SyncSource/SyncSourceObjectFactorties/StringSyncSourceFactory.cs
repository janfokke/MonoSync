using MonoSync.SyncSource.SyncSourceObjects;

namespace MonoSync.SyncSource.SyncSourceObjectFactorties
{
    internal class StringSyncSourceFactory : ISyncSourceFactory
    {
        public bool CanCreate(object baseType)
        {
            return baseType is string;
        }

        public SyncSource Create(SyncSourceRoot syncSourceRoot, int referenceId, object baseType,
            IFieldSerializerResolver fieldSerializerResolver)
        {
            return new StringSyncSource(syncSourceRoot, referenceId, baseType as string);
        }
    }
}