namespace MonoSync
{
    public interface ISyncSourceFactory
    {
        bool CanCreate(object baseType);

        SyncSource Create(SyncSourceRoot syncSourceRoot, int referenceId, object baseType,
            IFieldSerializerResolver fieldSerializerResolver);
    }
}