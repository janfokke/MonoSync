using System;

namespace MonoSync.Synchronizers
{
    internal class ObservableHashSetSynchronizer : ISynchronizer
    {
        public bool CanSynchronize(Type type)
        {
            if (type.IsGenericType)
            {
                return type.GetGenericTypeDefinition() == typeof(Collections.ObservableHashSet<>);
            }
            return false;
        }

        public SourceSynchronizer Synchronize(SourceSynchronizerRoot sourceSynchronizerRoot, int referenceId, object reference)
        {
            Type[] genericArgs = reference.GetType().GetGenericArguments();
            Type observableHashSetSyncSourceObjectType = typeof(ObservableHashSetSourceSynchronizer<>).MakeGenericType(genericArgs);
            return (SourceSynchronizer)Activator.CreateInstance(observableHashSetSyncSourceObjectType, sourceSynchronizerRoot,
                referenceId, reference);
        }

        public TargetSynchronizer Synchronize(TargetSynchronizerRoot targetSynchronizerRoot, int referenceId, Type referenceType)
        {
            Type[] genericArgs = referenceType.GetGenericArguments();
            Type observableHashSetTargetType = typeof(ObservableHashSetTargetSynchronizer<>).MakeGenericType(genericArgs);
            return (TargetSynchronizer)Activator.CreateInstance(observableHashSetTargetType, targetSynchronizerRoot, 
                referenceId, referenceType);
        }
    }
}