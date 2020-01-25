using MonoSync.SyncSource;

namespace MonoSync.Sample.Tweening
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