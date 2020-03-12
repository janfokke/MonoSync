namespace MonoSync
{
    public class TargetFieldSerializerResolverFactory : ITargetFieldSerializerResolverFactory,
        ISourceFieldSerializerResolverFactory
    {
        public IFieldSerializerResolver Create(IIdentifierResolver identifierResolver)
        {
            return new FieldSerializerResolver(identifierResolver);
        }

        public IFieldSerializerResolver Create(IReferenceResolver referenceResolver)
        {
            return new FieldSerializerResolver(referenceResolver);
        }
    }
}