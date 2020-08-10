using System;
using System.Collections.Generic;
using System.Linq;
using MonoSync.Attributes;

namespace MonoSync.Synchronizers
{
    internal class ObjectSynchronizer : ISynchronizer
    {
        public bool CanSynchronize(Type type)
        {
            return true;
        }

        public SourceSynchronizer Synchronize(SourceSynchronizerRoot sourceSynchronizerRoot, int referenceId, object reference)
        {
            return new ObjectSourceSynchronizer(sourceSynchronizerRoot, referenceId, reference);
        }

        public TargetSynchronizer Synchronize(TargetSynchronizerRoot targetSynchronizerRoot, int referenceId, Type referenceType)
        {
            return new ObjectTargetSynchronizer(targetSynchronizerRoot, referenceId, referenceType);
        }
    }
}