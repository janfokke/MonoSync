using System;
using System.Collections.Generic;
using MonoSync.Exceptions;
using MonoSync.SyncSourceObjectFactorties;

namespace MonoSync
{
    public class SyncSourceFactoryResolver : ISyncSourceFactoryResolver
    {
        private readonly List<ISyncSourceFactory> _syncSourceObjectFactories =
            new List<ISyncSourceFactory>();

        public Dictionary<Type, ISyncSourceFactory> FactoriesByType = new Dictionary<Type, ISyncSourceFactory>();

        public SyncSourceFactoryResolver()
        {
            AddSyncSourceObjectFactory(new NotifyPropertyChangedSyncSourceFactory());
            AddSyncSourceObjectFactory(new StringSyncSourceFactory());
            AddSyncSourceObjectFactory(new ObservableDictionarySyncSourceFactory());
        }

        public ISyncSourceFactory FindMatchingSyncSourceFactory(object baseObject)
        {
            Type type = baseObject.GetType();
            if (FactoriesByType.TryGetValue(type, out ISyncSourceFactory factory) == false)
            {
                // Factories are looped in reverse because the last added Factory should be prioritized.
                for (var i = _syncSourceObjectFactories.Count - 1; i >= 0; i--)
                {
                    ISyncSourceFactory syncSourceObjectFactory = _syncSourceObjectFactories[i];
                    if (syncSourceObjectFactory.CanCreate(baseObject))
                    {
                        factory = syncSourceObjectFactory;
                        FactoriesByType.Add(type, syncSourceObjectFactory);
                        return factory;
                    }
                }

                throw new SyncObjectFactoryNotFoundException(baseObject);
            }

            return factory;
        }

        public void AddSyncSourceObjectFactory(ISyncSourceFactory syncTargetFactory)
        {
            _syncSourceObjectFactories.Add(syncTargetFactory);
        }
    }
}