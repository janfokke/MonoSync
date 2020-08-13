using System;
using System.Collections.Generic;
using System.Reflection;
using MonoSync.Exceptions;
using MonoSync.Utils;

namespace MonoSync.Synchronizers
{
    public class ObjectSourceSynchronizer : SourceSynchronizer
    {
        protected readonly SynchronizableSourceMember[] SynchronizableSourceMembers;

        public ObjectSourceSynchronizer(SourceSynchronizerRoot sourceSynchronizerRoot, int referenceId,
            object reference) :
            base(sourceSynchronizerRoot, referenceId, reference)
        {
            Type type = Reference.GetType();
            SynchronizableMember[] synchronizableMembers = sourceSynchronizerRoot.SynchronizableMemberFactory.FromType(type);
            SynchronizableSourceMembers = new SynchronizableSourceMember[synchronizableMembers.Length];
            for (var i = 0; i < synchronizableMembers.Length; i++)
            {
                SynchronizableMember synchronizableMember = synchronizableMembers[i];
                var synchronizableSourceMember = new SynchronizableSourceMember(Reference, synchronizableMember);
                SynchronizableSourceMembers[i] = synchronizableSourceMember;
            }
            
            for (var i = 0; i < SynchronizableSourceMembers.Length; i++)
            {
                SynchronizableSourceMember synchronizableSourceMember = SynchronizableSourceMembers[i];
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
            for (var index = 0; index < SynchronizableSourceMembers.Length; index++)
            {
                SynchronizableSourceMembers[index].Serialize(binaryWriter);
            }
        }
    }
}