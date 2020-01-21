using System;
using System.Collections.Generic;
using MonoSync.Exceptions;
using MonoSync.SyncTarget.SyncTargetFactories;

namespace MonoSync.SyncTarget
{
    public class SyncTargetFactoryResolver : ISyncTargetFactoryResolver
    {
        private readonly List<ISyncTargetFactory> _syncTargetObjectFactories =
            new List<ISyncTargetFactory>();

        public SyncTargetFactoryResolver()
        {
            // Order is important, because ObservableDictionarySyncTargetObjectFactory also inherits INotifyPropertyChanged
            AddSyncTargetObjectFactory(new SyncSyncTargetFactory());
            AddSyncTargetObjectFactory(new ObservableDictionarySyncTargetFactory());
            AddSyncTargetObjectFactory(new StringSyncTargetFactory());
        }

        public ISyncTargetFactory FindMatchingSyncTargetObjectFactory(Type baseType)
        {
            // Factories are looped in reverse because the last added Factory should be prioritized.
            for (int i = _syncTargetObjectFactories.Count - 1; i >= 0; i--)
            {
                if (_syncTargetObjectFactories[i].CanCreate(baseType))
                {
                    return _syncTargetObjectFactories[i];
                }
            }

            throw new SyncTargetObjectFactoryNotFoundException(baseType);
        }

        public void AddSyncTargetObjectFactory(ISyncTargetFactory syncTargetFactory)
        {
            _syncTargetObjectFactories.Add(syncTargetFactory);
        }
    }
}