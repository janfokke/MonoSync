namespace MonoSync.Exceptions
{
    public class SyncTargetMemberNotFoundException : MonoSyncException
    {
        public SyncTargetMemberNotFoundException(string propertyName) : base(
            $"Could not find property with name: {propertyName}")
        {
        }
    }
}