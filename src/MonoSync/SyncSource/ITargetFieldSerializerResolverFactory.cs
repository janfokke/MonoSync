namespace MonoSync
{
    public interface ITargetFieldSerializerResolverFactory
    {
        IFieldSerializerResolver Create(IReferenceResolver referenceResolver);
    }
}