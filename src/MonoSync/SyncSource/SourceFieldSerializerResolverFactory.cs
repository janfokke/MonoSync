namespace MonoSync
{
    public class SourceFieldSerializerResolverFactory : ISourceFieldSerializerResolverFactory
    {
        public IFieldSerializerResolver Create(IIdentifierResolver identifierResolver)
        {
            return new FieldSerializerResolver(identifierResolver);
        }
    }
}