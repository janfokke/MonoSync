using System;

namespace MonoSync.Synchronizers
{
    internal class StringSynchronizer : ISynchronizer
    {
        public bool CanSynchronize(Type type)
        {
            return type == typeof(string);
        }

        public SourceSynchronizer Synchronize(SourceSynchronizerRoot sourceSynchronizerRoot, int referenceId, object reference)
        {
            return new StringSourceSynchronizer(sourceSynchronizerRoot, referenceId, (string)reference);
        }

        public TargetSynchronizer Synchronize(TargetSynchronizerRoot targetSynchronizerRoot, int referenceId, Type referenceType)
        {
            return new StringTargetSynchronizer(referenceId);
        }
    }
}