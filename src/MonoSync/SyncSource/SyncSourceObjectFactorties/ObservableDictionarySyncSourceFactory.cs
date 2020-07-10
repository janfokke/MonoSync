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

        public SynchronizerSource Create(SyncSourceRoot syncSourceRoot, int referenceId, object baseType)
        {
            Type[] genericArgs = baseType.GetType().GetGenericArguments();
            Type observableDictionarySyncSourceObjectType =
                typeof(ObservableDictionarySource<,>).MakeGenericType(genericArgs);
            return (SynchronizerSource) Activator.CreateInstance(observableDictionarySyncSourceObjectType, syncSourceRoot,
                referenceId,
                baseType, fieldSerializerResolver);
        }
    }
}