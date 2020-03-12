namespace MonoSync
{
    public class SyncSourceSettings
    {
        public static SyncSourceSettings Default => new SyncSourceSettings
        {
            SyncSourceFactoryResolver = new SyncSourceFactoryResolver(),
            SourceFieldDeserializerResolverFactory = new SourceFieldSerializerResolverFactory(),
            TypeEncoder = new TypeEncoder()
        };

        public ISourceFieldSerializerResolverFactory SourceFieldDeserializerResolverFactory { get; set; }
        public ITypeEncoder TypeEncoder { get; set; }
        public ISyncSourceFactoryResolver SyncSourceFactoryResolver { get; set; }
    }
}