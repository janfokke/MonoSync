using MonoSync.SyncTarget;

namespace MonoSync.Exceptions
{
    public class SyncTargetObjectFactoryNotFoundException : MonoSyncException
    {
        public SyncTargetObjectFactoryNotFoundException(object baseType) : base(
            $"Could not find {nameof(ISyncTargetFactory)} for {baseType}")
        {
        }
    }
}