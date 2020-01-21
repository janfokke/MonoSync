using System.ComponentModel;
using MonoSync.SyncSource.SyncSourceObjects;

namespace MonoSync.SyncSource.SyncSourceObjectFactorties
{
    internal class NotifyPropertyChangedSyncSourceFactory : ISyncSourceFactory
    {
        public bool CanCreate(object baseType)
        {
            return baseType is INotifyPropertyChanged;
        }

        public SyncSource Create(SyncSourceRoot syncSourceRoot, int referenceId, object baseType,
            IFieldSerializerResolver fieldSerializerResolver)
        {
            return new NotifyPropertyChangedSyncSource(syncSourceRoot, referenceId, baseType as INotifyPropertyChanged,
                fieldSerializerResolver);
        }
    }
}