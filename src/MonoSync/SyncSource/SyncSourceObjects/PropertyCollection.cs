using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MonoSync.Attributes;
using MonoSync.Utils;

namespace MonoSync.SyncSourceObjects
{
    public class PropertyCollection
    {
        private readonly Dictionary<string, SyncSourceProperty> _propertiesByName;

        private readonly SyncSourceProperty[] _syncSourceProperties;

        public int Length { get; }
        public SyncSourceProperty this[int index] => _syncSourceProperties[index];

        private PropertyCollection(SyncSourceProperty[] syncSourceProperties)
        {
            _syncSourceProperties = syncSourceProperties;
            _propertiesByName = syncSourceProperties.ToDictionary(x => x.Name);
            Length = _syncSourceProperties.Length;
        }

        public bool TryGetByName(string propertyName, out SyncSourceProperty syncSourceProperty)
        {
            return _propertiesByName.TryGetValue(propertyName, out syncSourceProperty);
        }

        public class Factory
        {
            private readonly IFieldSerializerResolver _fieldSerializerResolver;

            private readonly Dictionary<Type, PropertyCollection> _typeCache =
                new Dictionary<Type, PropertyCollection>();

            public Factory(IFieldSerializerResolver fieldSerializerResolver)
            {
                _fieldSerializerResolver = fieldSerializerResolver;
            }

            public PropertyCollection FromType(Type type)
            {
                if (_typeCache.TryGetValue(type, out PropertyCollection cachedPropertyCollection) == false)
                {
                    List<PropertyInfo> syncProperties = GetSyncProperties(type);
                    var syncSourceProperties = new SyncSourceProperty[syncProperties.Count];
                    for (short syncPropertyIndex = 0; syncPropertyIndex < syncProperties.Count; syncPropertyIndex++)
                    {
                        PropertyInfo syncSyncPropertyInfo = syncProperties[syncPropertyIndex];
                        Type propertyType = syncSyncPropertyInfo.PropertyType;
                        var property = new SyncSourceProperty(syncPropertyIndex, syncSyncPropertyInfo.Name,
                            _fieldSerializerResolver.ResolveSerializer(propertyType),
                            propertyType.IsValueType);
                        syncSourceProperties[syncPropertyIndex] = property;
                    }

                    cachedPropertyCollection = new PropertyCollection(syncSourceProperties);
                    _typeCache.Add(type, cachedPropertyCollection);
                }

                return cachedPropertyCollection;
            }

            private List<PropertyInfo> GetSyncProperties(Type type)
            {
                var result = new List<PropertyInfo>();
                PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (PropertyInfo propertyInfo in properties)
                {
                    var hasSyncAttribute = propertyInfo
                        .GetCustomAttributes(true)
                        .Any(x => x is SyncAttribute);

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