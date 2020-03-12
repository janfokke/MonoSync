using System;
using System.Collections.Generic;
using MonoSync.Exceptions;
using MonoSync.SyncTargetFactories;

namespace MonoSync
{
    public class SyncTargetFactoryResolver : ISyncTargetFactoryResolver
    {
        private readonly List<ISyncTargetFactory> _syncTargetObjectFactories =
            new List<ISyncTargetFactory>();

        public Dictionary<Type, ISyncTargetFactory> FactoriesByType = new Dictionary<Type, ISyncTargetFactory>();

        public SyncTargetFactoryResolver()
        {
            // Order is important, because ObservableDictionarySyncTargetObjectFactory also inherits INotifyPropertyChanged
            AddSyncTargetObjectFactory(new NotifyPropertyChangedSyncTargetFactory());
            AddSyncTargetObjectFactory(new ObservableDictionarySyncTargetFactory());
            AddSyncTargetObjectFactory(new StringSyncTargetFactory());
        }

        public ISyncTargetFactory FindMatchingSyncTargetObjectFactory(Type baseType)
        {
            if (FactoriesByType.TryGetValue(baseType, out ISyncTargetFactory factory) == false)
            {
                // Factories are looped in reverse because the last added Factory should be prioritized.
                for (var i = _syncTargetObjectFactories.Count - 1; i >= 0; i--)
                {
                    ISyncTargetFactory syncTargetObjectFactory = _syncTargetObjectFactories[i];
                    if (syncTargetObjectFactory.CanCreate(baseType))
                    {
                        factory = syncTargetObjectFactory;
                        FactoriesByType.Add(baseType, syncTargetObjectFactory);
                        return factory;
                    }
                }

                throw new SyncObjectFactoryNotFoundException(baseType);
            }

            return factory;
        }

        public void AddSyncTargetObjectFactory(ISyncTargetFactory syncTargetFactory)
        {
            _syncTargetObjectFactories.Add(syncTargetFactory);
        }
    }
}