

namespace MonoSync.Sample.Tweening
{
    public class TweenGameFieldSerializerFactory : ISourceFieldSerializerResolverFactory, ITargetFieldSerializerResolverFactory
    {
        public IFieldSerializerResolver Create(IReferenceResolver referenceResolver)
        {
            return new TweenGameFieldSerializers(referenceResolver);
        }

        public IFieldSerializerResolver Create(IIdentifierResolver identifierResolver)
        {
            return new TweenGameFieldSerializers(identifierResolver);
        }
    }
}