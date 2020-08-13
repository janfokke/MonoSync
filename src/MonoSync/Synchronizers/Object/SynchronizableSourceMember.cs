using System;
using MonoSync.Utils;

namespace MonoSync.Synchronizers
{
    public class SynchronizableSourceMember
    {
        private readonly object _declaringReference;
        private readonly SynchronizableMember _synchronizableMember;
        
        public object Value => _synchronizableMember.GetValue(_declaringReference);
        public bool IsValueType => _synchronizableMember.MemberType.IsValueType;
        public string Name => _synchronizableMember.MemberInfo.Name;
        public int Index => _synchronizableMember.Index;

        public SynchronizableSourceMember(object declaringReference,
            SynchronizableMember synchronizableMember)
        {
            _declaringReference = declaringReference;
            _synchronizableMember = synchronizableMember;
        }

        public void Serialize(ExtendedBinaryWriter writer)
        {
            _synchronizableMember.Serializer.Write(Value, writer);
        }
    }
}