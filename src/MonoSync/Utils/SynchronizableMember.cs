using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MonoSync.Attributes;
using MonoSync.Exceptions;
using MonoSync.Synchronizers;

namespace MonoSync.Utils
{
    public class SynchronizableMember
    {
        private readonly Func<object, object> _getter;
        private readonly Action<object, object> _setter;
        private static readonly ConcurrentDictionary<Type, SynchronizableMember[]> SynchronizableMemberCache = new ConcurrentDictionary<Type, SynchronizableMember[]>();

        public int Index { get; }
        public MemberInfo MemberInfo { get; }

        public Type MemberType => MemberInfo.MemberType switch
        {
            MemberTypes.Field => ((FieldInfo) MemberInfo).FieldType,
            MemberTypes.Property => ((PropertyInfo) MemberInfo).PropertyType,
        };
        
        public SynchronizeAttribute SynchronizeAttribute { get; }
        public bool CanSet => _setter != null;
        
        private SynchronizableMember(
            int index,
            MemberInfo memberInfo, 
            SynchronizeAttribute synchronizeAttribute, 
            Func<object, object> getter, 
            Action<object,object> setter)
        {
            _getter = getter;
            _setter = setter;

            Index = index;
            MemberInfo = memberInfo;
            SynchronizeAttribute = synchronizeAttribute;
        }

        public static SynchronizableMember[] FromType(Type type)
        {
            return SynchronizableMemberCache.GetOrAdd(type, key =>
            {
                var synchronizableMemberAccessors = new List<SynchronizableMember>();
                ResolvePropertyAccessors(key, synchronizableMemberAccessors);
                ResolveFieldAccessors(key, synchronizableMemberAccessors);
                return synchronizableMemberAccessors.ToArray();
            });
        }

        private static void ResolvePropertyAccessors(Type type, List<SynchronizableMember> synchronizableMembers)
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
                synchronizableMembers.Add(new SynchronizableMember(synchronizableMembers.Count, propertyInfo, synchronizeAttribute, getter, setter));
            }
        }

        private static void ResolveFieldAccessors(Type type, List<SynchronizableMember> synchronizableMembers)
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
                synchronizableMembers.Add(new SynchronizableMember(synchronizableMembers.Count, fieldInfo, synchronizeAttribute, getter, setter));
            }
        }

        private static SynchronizeAttribute FindSynchronizeAttribute(MemberInfo memberInfo)
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

        public object GetValue(object reference)
        {
            return _getter(reference);
        }

        public void SetValue(object reference, object value)
        {
            if (CanSet == false)
            {
                throw new SetterNotAvailableException(MemberInfo);
            }
            _setter(reference, value);
        }
    }
}