using System;
using FastMember;
using MonoSync.Utils;

namespace MonoSync.Synchronizers
{
    public class ObjectSourceSynchronizer : SourceSynchronizer
    {
        protected readonly SyncPropertyCollection SyncPropertyCollection;
        protected readonly TypeAccessor TypeAccessor;

        public ObjectSourceSynchronizer(SourceSynchronizerRoot sourceSynchronizerRoot, int referenceId,
            object reference) :
            base(sourceSynchronizerRoot, referenceId, reference)
        {
            Type baseType = reference.GetType();
            TypeAccessor = TypeAccessor.Create(baseType, true);
            SyncPropertyCollection = sourceSynchronizerRoot.GetPropertiesFromType(baseType);
            
            for (var i = 0; i < SyncPropertyCollection.Length; i++)
            {
                SyncSourceProperty syncSourceProperty = SyncPropertyCollection[i];
                if (!syncSourceProperty.IsValueType)
                {
                    object initialValue = TypeAccessor[Reference, syncSourceProperty.Name];
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
            for (var index = 0; index < SyncPropertyCollection.Length; index++)
            {
                SyncSourceProperty sourceProperty = SyncPropertyCollection[index];
                object value = TypeAccessor[Reference, sourceProperty.Name];
                sourceProperty.Serializer.Write(value, binaryWriter);
            }
        }
    }
}