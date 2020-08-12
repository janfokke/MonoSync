namespace MonoSync.Sample.Tweening
{
    public class TweenGameFieldSerializers : FieldSerializerResolver
    {
        public TweenGameFieldSerializers(IReferenceResolver referenceResolver) : base(referenceResolver)
        {
            Initialize();
        }

        public TweenGameFieldSerializers(IIdentifierResolver identifierResolver) : base(identifierResolver)
        {
            Initialize();
        }

        private void Initialize()
        {
            AddSerializer(new Vector2Serializer());
            AddSerializer(new ColorSerializer());
        }
    }
}