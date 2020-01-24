using MonoSync;
using MonoSync.SyncSource;

namespace Tweening
{
    public class TweenGameFieldSerializerFactory : IFieldSerializerResolverFactory
    {
        public IFieldSerializerResolver Create(IReferenceResolver referenceResolver)
        {
            return new TweenGameFieldSerializers(referenceResolver);
        }
    }
}