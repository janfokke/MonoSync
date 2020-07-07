namespace MonoSync.Sample.Tweening
{
    public class TweenGameSerializers : SerializerCollection
    {
        public TweenGameSerializers(IReferenceResolver referenceResolver) : base(referenceResolver)
        {
            Initialize();
        }

        public TweenGameSerializers(IIdentifierResolver identifierResolver) : base(identifierResolver)
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