using System;
using MonoSync.Utils;

namespace MonoSync
{
    public interface ISynchronizer
    {
        bool CanSynchronize(Type type);

        SourceSynchronizer Synchronize(SourceSynchronizerRoot sourceSynchronizerRoot, int referenceId, object reference);
        TargetSynchronizer Synchronize(TargetSynchronizerRoot targetSynchronizerRoot, int referenceId, Type referenceType);
    }
}