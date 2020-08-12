using System;

namespace MonoSync.Exceptions
{
    public class SynchronizerNotFoundException : MonoSyncException
    {
        public SynchronizerNotFoundException(Type type) : base(
            $"Could not find {nameof(ISynchronizer)} for {type}")
        {
        }
    }
}