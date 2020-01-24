using MonoSync;
using MonoSync.SyncSource;

namespace Tweening
{
    public class TweenGameFieldSerializers : FieldSerializerResolver
    {
        public TweenGameFieldSerializers(IReferenceResolver referenceResolver) : base(referenceResolver)
        {
            AddSerializer(new Vector2Serializer());
            AddSerializer(new ColorSerializer());
        }
    }
}