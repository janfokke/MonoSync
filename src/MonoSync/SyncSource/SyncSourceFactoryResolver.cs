using System.Collections.Generic;
using MonoSync.Exceptions;
using MonoSync.SyncSource.SyncSourceObjectFactorties;

namespace MonoSync.SyncSource
{
    public class SyncSourceFactoryResolver : ISyncSourceFactoryResolver
    {
        private readonly List<ISyncSourceFactory> _syncSourceObjectFactories =
            new List<ISyncSourceFactory>();

        public SyncSourceFactoryResolver()
        {
            AddSyncSourceObjectFactory(new NotifyPropertyChangedSyncSourceFactory());
            AddSyncSourceObjectFactory(new StringSyncSourceFactory());
            AddSyncSourceObjectFactory(new ObservableDictionarySyncSourceFactory());
        }

        public ISyncSourceFactory FindMatchingSyncSourceFactory(object baseObject)
        {
            // Factories are looped in reverse because the last added Factory should be prioritized.
            for (int i = _syncSourceObjectFactories.Count - 1; i >= 0; i--)
            {
                if (_syncSourceObjectFactories[i].CanCreate(baseObject))
                {
                    return _syncSourceObjectFactories[i];
                }
            }

            throw new SyncTargetObjectFactoryNotFoundException(baseObject);
        }

        public void AddSyncSourceObjectFactory(ISyncSourceFactory syncTargetFactory)
        {
            _syncSourceObjectFactories.Add(syncTargetFactory);
        }
    }
}