using System;
using System.Reflection;
using MonoSync.Exceptions;
using MonoSync.Utils;

namespace MonoSync.Synchronizers
{
    public class ObjectSourceSynchronizer : SourceSynchronizer
    {
        protected readonly SourceMemberCollection SourceMemberCollection;

        public ObjectSourceSynchronizer(SourceSynchronizerRoot sourceSynchronizerRoot, int referenceId,
            object reference) :
            base(sourceSynchronizerRoot, referenceId, reference)
        {
            Type baseType = reference.GetType();

            Type type = Reference.GetType();
            SynchronizableMember[] synchronizableMembers = SynchronizableMember.FromType(type);

            var synchronizableSourceMembers = new SynchronizableSourceMember[synchronizableMembers.Length];
            for (var i = 0; i < synchronizableMembers.Length; i++)
            {
                SynchronizableMember synchronizableMember = synchronizableMembers[i];
                var synchronizableSourceMember = new SynchronizableSourceMember(Reference, synchronizableMember, sourceSynchronizerRoot);
                synchronizableSourceMembers[i] = synchronizableSourceMember;
            }
            SourceMemberCollection = new SourceMemberCollection(synchronizableSourceMembers);

            for (var i = 0; i < SourceMemberCollection.Length; i++)
            {
                SynchronizableSourceMember synchronizableSourceMember = SourceMemberCollection[i];
                if (!synchronizableSourceMember.IsValueType)
                {
                    object initialValue = synchronizableSourceMember.Value;
                    if (initialValue != null)
                    {
                        sourceSynchronizerRoot.Synchronize(initialValue);
                    }
                }
            }
        }

        public override void WriteChanges(ExtendedBinaryWriter binaryWriter)
        {
            throw new NotImplementedException();
        }

        public override void WriteFull(ExtendedBinaryWriter binaryWriter)
        {
            for (var index = 0; index < SourceMemberCollection.Length; index++)
            {
                SourceMemberCollection[index].Serialize(binaryWriter);
            }
        }
    }
}