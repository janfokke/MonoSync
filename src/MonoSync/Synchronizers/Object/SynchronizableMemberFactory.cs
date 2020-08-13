using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using MonoSync.Attributes;

namespace MonoSync.Utils
{
    public class SynchronizableMemberFactory
    {
        private readonly SerializerCollection _serializerCollection;
        private readonly ConcurrentDictionary<Type, SynchronizableMember[]> _synchronizableMemberCache = new ConcurrentDictionary<Type, SynchronizableMember[]>();

        public SynchronizableMemberFactory(SerializerCollection serializerCollection)
        {
            _serializerCollection = serializerCollection;
        }

        public SynchronizableMember[] FromType(Type type)
        {
            return _synchronizableMemberCache.GetOrAdd(type, key =>
            {
                var synchronizableMemberAccessors = new List<SynchronizableMember>();
                ResolvePropertyAccessors(key, synchronizableMemberAccessors);
                ResolveFieldAccessors(key, synchronizableMemberAccessors);
                return synchronizableMemberAccessors.ToArray();
            });
        }

        private void ResolvePropertyAccessors(Type type, List<SynchronizableMember> synchronizableMembers)
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (var i = 0; i < properties.Length; i++)
            {
                PropertyInfo propertyInfo = properties[i];
                SynchronizeAttribute synchronizeAttribute = FindSynchronizeAttribute(propertyInfo);
                if (synchronizeAttribute == null)
                {
                    continue;
                }
                Func<object, object> getter = ReflectionUtils.CompilePropertyGetter(propertyInfo);
                Action<object, object> setter = ReflectionUtils.CompilePropertySetter(propertyInfo);
                ISerializer serializer = _serializerCollection.FindSerializerByType(propertyInfo.PropertyType);
                synchronizableMembers.Add(new SynchronizableMember(synchronizableMembers.Count, serializer, propertyInfo, synchronizeAttribute, getter, setter));
            }
        }

        private void ResolveFieldAccessors(Type type, List<SynchronizableMember> synchronizableMembers)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (var i = 0; i < fields.Length; i++)
            {
                FieldInfo fieldInfo = fields[i];
                SynchronizeAttribute synchronizeAttribute = FindSynchronizeAttribute(fieldInfo);
                if (synchronizeAttribute == null)
                {
                    continue;
                }
                Func<object, object> getter = ReflectionUtils.CompileFieldGetter(fieldInfo);
                Action<object, object> setter = ReflectionUtils.CompileFieldSetter(fieldInfo);
                ISerializer serializer = _serializerCollection.FindSerializerByType(fieldInfo.FieldType);
                synchronizableMembers.Add(new SynchronizableMember(synchronizableMembers.Count, serializer, fieldInfo, synchronizeAttribute, getter, setter));
            }
        }

        private SynchronizeAttribute FindSynchronizeAttribute(MemberInfo memberInfo)
        {
            object[] attributes = memberInfo.GetCustomAttributes(true);
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