namespace MonoSync.SyncSource
{
    internal class FieldSerializerResolverFactory : IFieldSerializerResolverFactory
    {
        public IFieldSerializerResolver Create(IReferenceResolver referenceResolver)
        {
            return new FieldSerializerResolver(referenceResolver);
        }
    }
}