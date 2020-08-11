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
            SourceMemberCollection = InitializeSourceMemberCollection();
            
            for (var i = 0; i < SourceMemberCollection.Length; i++)
            {
                SyncSourceProperty syncSourceProperty = SourceMemberCollection[i];
                if (!syncSourceProperty.IsValueType)
                {
                    object initialValue = syncSourceProperty.Value;
                    if (initialValue != null)
                    {
                        sourceSynchronizerRoot.Synchronize(initialValue);
                    }
                }
            }
        }

        private SourceMemberCollection InitializeSourceMemberCollection()
        {
            Type type = Reference.GetType();

            SynchronizablePropertyInfo[] synchronizableProperties = SynchronizablePropertyInfo.FromType(type);

            var syncSourceProperties = new SyncSourceProperty[synchronizableProperties.Length];

            for (short index = 0; index < synchronizableProperties.Length; index++)
            {
                SynchronizablePropertyInfo synchronizablePropertyInfo = synchronizableProperties[index];
                PropertyInfo propertyInfo = synchronizablePropertyInfo.PropertyInfo;

                Type propertyType = propertyInfo.PropertyType;

                if (ReflectionUtils.TryResolvePropertyGetter(out Func<object, object> getter, propertyInfo))
                {
                    var property = new SyncSourceProperty(index, propertyInfo.Name,
                        SourceSynchronizerRoot.Settings.Serializers.FindSerializerByType(propertyType),
                        propertyType.IsValueType, () => getter(Reference));
                    syncSourceProperties[index] = property;
                }
                else
                {
                    throw new GetterNotFoundException(propertyInfo.Name);
                }
            }
            return new SourceMemberCollection(syncSourceProperties);
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