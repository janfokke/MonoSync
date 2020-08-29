using MonoSync.Exceptions;

namespace MonoSync
{
    public class ObjectTargetSynchronizerNotFoundException : MonoSyncException
    {
        public ObjectTargetSynchronizerNotFoundException() : base("ObjectTargetSynchronizer not found")
        {
        }
    }
}