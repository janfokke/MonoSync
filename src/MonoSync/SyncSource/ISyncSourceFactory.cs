namespace MonoSync
{
    public interface ISyncSourceFactory
    {
        bool CanCreate(object baseType);

        SynchronizerSource Create(SyncSourceRoot syncSourceRoot, int referenceId, object baseType);
    }
}