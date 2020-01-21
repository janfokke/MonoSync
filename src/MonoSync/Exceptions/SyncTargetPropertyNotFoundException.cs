namespace MonoSync.Exceptions
{
    public class SyncTargetPropertyNotFoundException : MonoSyncException
    {
        public SyncTargetPropertyNotFoundException(string propertyName) : base(
            $"Could not find property with name: {propertyName}")
        {
        }
    }
}