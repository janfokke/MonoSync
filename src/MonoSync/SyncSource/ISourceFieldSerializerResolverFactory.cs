namespace MonoSync
{
    public interface ISourceFieldSerializerResolverFactory
    {
        IFieldSerializerResolver Create(IIdentifierResolver identifierResolver);
    }
}