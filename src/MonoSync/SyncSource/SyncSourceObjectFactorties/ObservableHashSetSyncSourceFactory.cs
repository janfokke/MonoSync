using System;
using MonoSync.Collections;
using MonoSync.SyncSourceObjects;

namespace MonoSync.SyncSourceObjectFactorties
{
    internal class ObservableHashSetSyncSourceFactory : ISyncSourceFactory
    {
        public bool CanCreate(object baseType)
        {
            Type type = baseType.GetType();
            if (type.IsGenericType)
            {
                return type.GetGenericTypeDefinition() == typeof(ObservableHashSet<>);
            }

            return false;
        }

        public SyncSource Create(SyncSourceRoot syncSourceRoot, int referenceId, object baseType,
            IFieldSerializerResolver fieldSerializerResolver)
        {
            Type[] genericArgs = baseType.GetType().GetGenericArguments();
            Type observableHashSetSyncSourceObjectType = typeof(ObservableHashSetSource<>).MakeGenericType(genericArgs);
            return (SyncSource)Activator.CreateInstance(observableHashSetSyncSourceObjectType, syncSourceRoot,
                referenceId,
                baseType, fieldSerializerResolver);
        }
    }
}