using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using MonoSync.Attributes;

namespace MonoSync.Utils
{
    internal class SyncPropertyInfo
    {
        public string Name { get; }
        public Type Type { get; }

        public SyncPropertyInfo(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }

    internal class PropertyInfoHelper
    {
        private static readonly ConcurrentDictionary<Type, List<(PropertyInfo, SyncAttribute)>> PropertyInfoCache =
            new ConcurrentDictionary<Type, List<(PropertyInfo, SyncAttribute)>>();

        public static List<(PropertyInfo, SyncAttribute)> GetSyncProperties(Type type)
        {
            return PropertyInfoCache.GetOrAdd(type, type =>
            {
                PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

                var syncProperties = new List<(PropertyInfo, SyncAttribute)>();
                for (var i = 0; i < properties.Length; i++)
                {
                    PropertyInfo propertyInfo = properties[i];
                    SyncAttribute syncAttribute = GetSyncAttribute(propertyInfo);
                    if (syncAttribute == null)
                    {
                        continue;
                    }

                    syncProperties.Add((properties[i], syncAttribute));
                }

                return syncProperties;
            });
        }

        private static SyncAttribute GetSyncAttribute(PropertyInfo propertyInfo)
        {
            object[] attributes = propertyInfo.GetCustomAttributes(true);
            foreach (object attribute in attributes)
            {
                if (attribute is SyncAttribute syncAttribute)
                {
                    return syncAttribute;
                }
            }

            return null;
        }
    }
}