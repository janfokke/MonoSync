using System;
using System.Collections.Generic;
using MonoSync.Exceptions;
using MonoSync.Synchronizers;

namespace MonoSync
{
    public class SynchronizerCollection
    {
        private readonly List<ISynchronizer> _syncSourceObjectFactories = new List<ISynchronizer>();
        private readonly Dictionary<Type, ISynchronizer> _factoriesByType = new Dictionary<Type, ISynchronizer>();

        public SynchronizerCollection()
        {
            AddSynchronizer(new ObjectSynchronizer());
            AddSynchronizer(new NotifyPropertyChangedSynchronizer());
            AddSynchronizer(new StringSynchronizer());
            AddSynchronizer(new ObservableDictionarySynchronizer());
            AddSynchronizer(new ObservableHashSetSynchronizer());
        }

        public ISynchronizer FindSynchronizerByType(Type type)
        {
            if (_factoriesByType.TryGetValue(type, out ISynchronizer factory) == false)
            {
                // Factories are looped in reverse because the last added Factory should be prioritized.
                for (var i = _syncSourceObjectFactories.Count - 1; i >= 0; i--)
                {
                    ISynchronizer syncSourceObjectFactory = _syncSourceObjectFactories[i];
                    if (syncSourceObjectFactory.CanSynchronize(type))
                    {
                        factory = syncSourceObjectFactory;
                        _factoriesByType.Add(type, syncSourceObjectFactory);
                        return factory;
                    }
                }

                throw new SynchronizerNotFoundException(type);
            }
            return factory;
        }

        public void AddSynchronizer(ISynchronizer syncTargetFactory)
        {
            _syncSourceObjectFactories.Add(syncTargetFactory);
        }
    }
}