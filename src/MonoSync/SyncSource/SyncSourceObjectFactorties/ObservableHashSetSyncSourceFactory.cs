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

        public SynchronizerSource Create(SyncSourceRoot syncSourceRoot, int referenceId, object baseType)
        {
            Type[] genericArgs = baseType.GetType().GetGenericArguments();
            Type observableHashSetSyncSourceObjectType = typeof(ObservableHashSetSource<>).MakeGenericType(genericArgs);
            return (SynchronizerSource)Activator.CreateInstance(observableHashSetSyncSourceObjectType, syncSourceRoot,
                referenceId,
                baseType, fieldSerializerResolver);
        }
    }
}