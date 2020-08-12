using System;
using MonoSync.Utils;

namespace MonoSync.Synchronizers
{
    public class SynchronizableSourceMember
    {
        private readonly object _declaringReference;
        private readonly SynchronizableMember _synchronizableMember;
        private readonly ISerializer _serializer;

        public object Value => _synchronizableMember.GetValue(_declaringReference);
        public bool IsValueType => _synchronizableMember.MemberType.IsValueType;
        public string Name => _synchronizableMember.MemberInfo.Name;
        public int Index => _synchronizableMember.Index;

        public SynchronizableSourceMember(object declaringReference,
            SynchronizableMember synchronizableMember,
            SourceSynchronizerRoot targetSynchronizerRoot)
        {
            _declaringReference = declaringReference;
            _synchronizableMember = synchronizableMember;
            _serializer = targetSynchronizerRoot.Settings.Serializers.FindSerializerByType(synchronizableMember.MemberType);
        }

        public void Serialize(ExtendedBinaryWriter writer)
        {
            _serializer.Write(Value, writer);
        }
    }
}