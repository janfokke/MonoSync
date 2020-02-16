namespace MonoSync.Exceptions
{
    public class SyncObjectFactoryNotFoundException : MonoSyncException
    {
        public SyncObjectFactoryNotFoundException(object baseType) : base(
            $"Could not find {nameof(ISyncTargetFactory)} for {baseType}")
        {
        }
    }
}