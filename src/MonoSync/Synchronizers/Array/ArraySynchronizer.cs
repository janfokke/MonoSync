using System;
using System.Collections.Generic;
using System.Text;

namespace MonoSync.Synchronizers
{
    class ArraySynchronizer : ISynchronizer
    {
        public bool CanSynchronize(Type type)
        {
            return type.IsArray;
        }

        public SourceSynchronizer Synchronize(SourceSynchronizerRoot sourceSynchronizerRoot, int referenceId, object reference)
        {
            return new ArraySourceSynchronizer(sourceSynchronizerRoot, referenceId, reference);
        }

        public TargetSynchronizer Synchronize(TargetSynchronizerRoot targetSynchronizerRoot, int referenceId, Type referenceType)
        {
            return new ArrayTargetSynchronizer(targetSynchronizerRoot, referenceId, referenceType);
        }
    }
}
