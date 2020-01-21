using System;
using MonoSync.Collections;
using MonoSync.SyncSource;
using MonoSync.SyncTarget.SyncTargetObjects;
using MonoSync.Utils;

namespace MonoSync.SyncTarget.SyncTargetFactories
{
    internal class ObservableDictionarySyncTargetFactory : ISyncTargetFactory
    {
        public bool CanCreate(Type baseType)
        {
            if (baseType.IsGenericType)
            {
                return baseType.GetGenericTypeDefinition() == typeof(ObservableDictionary<,>);
            }

            return false;
        }

        public SyncTarget Create(int referenceId, Type baseType, ExtendedBinaryReader reader,
            IFieldSerializerResolver fieldDeserializerResolver, SyncTargetRoot root)
        {
            Type[] genericArgs = baseType.GetGenericArguments();
            Type observableDictionarySyncSourceObjectType =
                typeof(ObservableDictionarySyncTarget<,>).MakeGenericType(genericArgs);
            return (SyncTarget) Activator.CreateInstance(observableDictionarySyncSourceObjectType, referenceId,
                baseType, reader, root, fieldDeserializerResolver);
        }
    }
}