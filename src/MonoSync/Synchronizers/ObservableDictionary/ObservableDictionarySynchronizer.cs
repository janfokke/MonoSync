using System;

namespace MonoSync.Synchronizers
{
    internal class ObservableDictionarySynchronizer : ISynchronizer
    {
        public bool CanSynchronize(Type type)
        {
            if (type.IsGenericType)
            {
                return type.GetGenericTypeDefinition() == typeof(Collections.ObservableDictionary<,>);
            }
            return false;
        }

        public SourceSynchronizer Synchronize(SourceSynchronizerRoot sourceSynchronizerRoot, int referenceId, object reference)
        {
            Type[] genericArgs = reference.GetType().GetGenericArguments();
            Type observableDictionarySyncSourceObjectType =
                typeof(ObservableDictionarySourceSynchronizer<,>).MakeGenericType(genericArgs);
            return (SourceSynchronizer) Activator.CreateInstance(observableDictionarySyncSourceObjectType, sourceSynchronizerRoot,
                referenceId, reference);
        }

        public TargetSynchronizer Synchronize(TargetSynchronizerRoot targetSynchronizerRoot, int referenceId, Type referenceType)
        {
            Type[] genericArgs = referenceType.GetGenericArguments();
            Type observableDictionarySyncSourceObjectType = typeof(ObservableDictionaryTargetSynchronizer<,>).MakeGenericType(genericArgs);
            return (TargetSynchronizer)Activator.CreateInstance(observableDictionarySyncSourceObjectType, targetSynchronizerRoot, referenceId, referenceType);
        }
    }
}