namespace MonoSync.SyncSource
{
    public interface IFieldSerializerResolverFactory
    {
        IFieldSerializerResolver Create(IReferenceResolver referenceResolver);
    }
}