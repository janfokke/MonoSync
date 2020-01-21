namespace MonoSync.SyncSource
{
    public class SyncSourceSettings
    {
        public static SyncSourceSettings Default => new SyncSourceSettings
        {
            SyncSourceFactoryResolver = new SyncSourceFactoryResolver(),
            FieldDeserializerResolverFactory = new FieldSerializerResolverFactory(),
            TypeEncoder = new TypeEncoder()
        };

        public IFieldSerializerResolverFactory FieldDeserializerResolverFactory { get; set; }
        public ITypeEncoder TypeEncoder { get; set; }
        public ISyncSourceFactoryResolver SyncSourceFactoryResolver { get; set; }
    }
}