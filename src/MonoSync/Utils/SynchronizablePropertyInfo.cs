using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using MonoSync.Attributes;

namespace MonoSync.Utils
{
    public class SynchronizablePropertyInfo
    {
        private SynchronizablePropertyInfo(SynchronizeAttribute synchronizeAttribute, PropertyInfo propertyInfo)
        {
            SynchronizeAttribute = synchronizeAttribute;
            PropertyInfo = propertyInfo;
        }

        public PropertyInfo PropertyInfo { get; }
        public SynchronizeAttribute SynchronizeAttribute { get; }

        private static readonly ConcurrentDictionary<Type, SynchronizablePropertyInfo[]> SyncPropertyInfoCache =
            new ConcurrentDictionary<Type, SynchronizablePropertyInfo[]>();

        public static SynchronizablePropertyInfo[] FromType(Type type)
        {
            return SyncPropertyInfoCache.GetOrAdd(type, key =>
            {
                PropertyInfo[] properties = key.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                var syncProperties = new List<SynchronizablePropertyInfo>();
                for (var i = 0; i < properties.Length; i++)
                {
                    PropertyInfo propertyInfo = properties[i];
                    SynchronizeAttribute synchronizeAttribute = GetSyncAttribute(propertyInfo);
                    if (synchronizeAttribute == null)
                    {
                        continue;
                    }
                    syncProperties.Add(new SynchronizablePropertyInfo(synchronizeAttribute, properties[i]));
                }
                return syncProperties.ToArray();
            });
        }

        private static SynchronizeAttribute GetSyncAttribute(PropertyInfo propertyInfo)
        {
            object[] attributes = propertyInfo.GetCustomAttributes(true);
            for (var i = 0; i < attributes.Length; i++)
            {
                if (attributes[i] is SynchronizeAttribute syncAttribute)
                {
                    return syncAttribute;
                }
            }
            return null;
        }
    }
}