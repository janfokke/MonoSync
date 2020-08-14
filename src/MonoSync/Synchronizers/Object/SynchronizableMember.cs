using System;
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
        
        public int Index { get; }
        public MemberInfo MemberInfo { get; }

        public Type MemberType { get; }
        
        public SynchronizeAttribute SynchronizeAttribute { get; }
        public bool CanSet => _setter != null;
        public ISerializer Serializer { get; }

        public SynchronizableMember(
            int index,
            ISerializer serializer,
            MemberInfo memberInfo, 
            SynchronizeAttribute synchronizeAttribute, 
            Func<object, object> getter, 
            Action<object,object> setter)
        {
            _getter = getter;
            _setter = setter;

            Index = index;
            Serializer = serializer;
            MemberInfo = memberInfo;

            MemberType = MemberInfo.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo)MemberInfo).FieldType,
                MemberTypes.Property => ((PropertyInfo)MemberInfo).PropertyType,
            };

            SynchronizeAttribute = synchronizeAttribute;
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