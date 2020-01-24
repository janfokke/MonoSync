using MonoSync.SyncSource;

namespace MonoSync.Sample.Tweening
{
    public class TweenGameFieldSerializerFactory : IFieldSerializerResolverFactory
    {
        public IFieldSerializerResolver Create(IReferenceResolver referenceResolver)
        {
            return new TweenGameFieldSerializers(referenceResolver);
        }
    }
}