using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MonoSync.Attributes;

namespace MonoSync.Synchronizers
{
    public class SyncPropertyCollection
    {
        private readonly Dictionary<string, SyncSourceProperty> _propertiesByName;
        private readonly SyncSourceProperty[] _syncSourceProperties;

        public int Length { get; }
        public SyncSourceProperty this[int index] => _syncSourceProperties[index];

        private SyncPropertyCollection(SyncSourceProperty[] syncSourceProperties)
        {
            _syncSourceProperties = syncSourceProperties;
            _propertiesByName = syncSourceProperties.ToDictionary(x => x.Name);
            Length = _syncSourceProperties.Length;
        }

        public bool TryGetPropertyByName(string propertyName, out SyncSourceProperty syncSourceProperty)
        {
            return _propertiesByName.TryGetValue(propertyName, out syncSourceProperty);
        }

        public class Factory
        {
            private readonly SerializerCollection _serializerCollection;

            private readonly Dictionary<Type, SyncPropertyCollection> _typeCache =
                new Dictionary<Type, SyncPropertyCollection>();

            public Factory(SerializerCollection serializerCollection)
            {
                _serializerCollection = serializerCollection;
            }

            public SyncPropertyCollection FromType(Type type)
            {
                if (_typeCache.TryGetValue(type, out SyncPropertyCollection cachedPropertyCollection) == false)
                {
                    List<PropertyInfo> syncProperties = GetSynchronizableProperties(type);
                    var syncSourceProperties = new SyncSourceProperty[syncProperties.Count];
                    for (short syncPropertyIndex = 0; syncPropertyIndex < syncProperties.Count; syncPropertyIndex++)
                    {
                        PropertyInfo syncSyncPropertyInfo = syncProperties[syncPropertyIndex];
                        Type propertyType = syncSyncPropertyInfo.PropertyType;
                        var property = new SyncSourceProperty(syncPropertyIndex, syncSyncPropertyInfo.Name,
                            _serializerCollection.FindSerializerByType(propertyType),
                            propertyType.IsValueType);
                        syncSourceProperties[syncPropertyIndex] = property;
                    }

                    cachedPropertyCollection = new SyncPropertyCollection(syncSourceProperties);
                    _typeCache.Add(type, cachedPropertyCollection);
                }
                return cachedPropertyCollection;
            }

            private List<PropertyInfo> GetSynchronizableProperties(Type type)
            {
                var result = new List<PropertyInfo>();
                PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (PropertyInfo propertyInfo in properties)
                {
                    var hasSyncAttribute = propertyInfo
                        .GetCustomAttributes(true)
                        .Any(x => x is SynchronizeAttribute);

                    if (hasSyncAttribute)
                    {
                        result.Add(propertyInfo);
                    }
                }

                return result;
            }
        }
    }
}