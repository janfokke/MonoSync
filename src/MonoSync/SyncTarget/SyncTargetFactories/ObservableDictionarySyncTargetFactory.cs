using System;
using MonoSync.Collections;
using MonoSync.SyncTargetObjects;
using MonoSync.Utils;

namespace MonoSync.SyncTargetFactories
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
                typeof(ObservableDictionaryTarget<,>).MakeGenericType(genericArgs);
            return (SyncTarget) Activator.CreateInstance(observableDictionarySyncSourceObjectType, referenceId,
                baseType, reader, root, fieldDeserializerResolver);
        }
    }
}