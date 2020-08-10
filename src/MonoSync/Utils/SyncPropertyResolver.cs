using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using MonoSync.Attributes;

namespace MonoSync.Utils
{
    internal class SyncPropertyResolver
    {
        private static readonly ConcurrentDictionary<Type, List<SyncPropertyInfo>> SyncPropertyInfoCache =
            new ConcurrentDictionary<Type, List<SyncPropertyInfo>>();

        public static List<SyncPropertyInfo> GetSyncProperties(Type type)
        {
            return SyncPropertyInfoCache.GetOrAdd(type, type =>
            {
                PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                var syncProperties = new List<SyncPropertyInfo>();
                for (var i = 0; i < properties.Length; i++)
                {
                    PropertyInfo propertyInfo = properties[i];
                    SynchronizeAttribute synchronizeAttribute = GetSyncAttribute(propertyInfo);
                    if (synchronizeAttribute == null)
                    {
                        continue;
                    }
                    syncProperties.Add(new SyncPropertyInfo(synchronizeAttribute,properties[i]));
                }

                return syncProperties;
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