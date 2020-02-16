using System;
using MonoSync.Collections;
using MonoSync.SyncSourceObjects;

namespace MonoSync.SyncSourceObjectFactorties
{
    internal class ObservableDictionarySyncSourceFactory : ISyncSourceFactory
    {
        public bool CanCreate(object baseType)
        {
            Type type = baseType.GetType();
            if (type.IsGenericType)
            {
                return type.GetGenericTypeDefinition() == typeof(ObservableDictionary<,>);
            }

            return false;
        }

        public SyncSource Create(SyncSourceRoot syncSourceRoot, int referenceId, object baseType,
            IFieldSerializerResolver fieldSerializerResolver)
        {
            Type[] genericArgs = baseType.GetType().GetGenericArguments();
            Type observableDictionarySyncSourceObjectType =
                typeof(ObservableDictionarySyncSource<,>).MakeGenericType(genericArgs);
            return (SyncSource) Activator.CreateInstance(observableDictionarySyncSourceObjectType, syncSourceRoot,
                referenceId,
                baseType, fieldSerializerResolver);
        }
    }
}