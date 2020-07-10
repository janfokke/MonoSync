using System.ComponentModel;
using MonoSync.SyncSourceObjects;

namespace MonoSync.SyncSourceObjectFactorties
{
    internal class NotifyPropertyChangedSyncSourceFactory : ISyncSourceFactory
    {
        public bool CanCreate(object baseType)
        {
            return baseType is INotifyPropertyChanged;
        }

        public SynchronizerSource Create(SyncSourceRoot syncSourceRoot, int referenceId, object baseType)
        {
            return new NotifyPropertyChangedSynchronizerSource(syncSourceRoot, referenceId, (INotifyPropertyChanged) baseType);
        }
    }
}