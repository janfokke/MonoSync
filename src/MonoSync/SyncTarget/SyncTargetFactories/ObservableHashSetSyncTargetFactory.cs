using System;
using MonoSync.Collections;
using MonoSync.SyncTargetObjects;
using MonoSync.Utils;

namespace MonoSync.SyncTargetFactories
{
    internal class ObservableHashSetSyncTargetFactory : ISyncTargetFactory
    {
        public bool CanCreate(Type baseType)
        {
            if (baseType.IsGenericType)
            {
                return baseType.GetGenericTypeDefinition() == typeof(ObservableHashSet<>);
            }
            return false;
        }

        public SynchronizerTarget Create(int referenceId, Type baseType, ExtendedBinaryReader reader,
            IFieldSerializerResolver fieldDeserializerResolver, SyncTargetRoot root)
        {
            Type[] genericArgs = baseType.GetGenericArguments();
            Type observableHashSetTargetType = typeof(ObservableHashSetTarget<>).MakeGenericType(genericArgs);
            return (SynchronizerTarget)Activator.CreateInstance(observableHashSetTargetType, referenceId,
                baseType, reader, root, fieldDeserializerResolver);
            
        }
    }
}