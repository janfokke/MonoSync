namespace MonoSync
{
    public interface ISyncSourceFactoryResolver
    {
        ISyncSourceFactory FindMatchingSyncSourceFactory(object baseObject);
    }
}