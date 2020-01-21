namespace MonoSync.SyncSource
{
    public interface ISyncSourceFactoryResolver
    {
        ISyncSourceFactory FindMatchingSyncSourceFactory(object baseObject);
    }
}