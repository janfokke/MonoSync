using MonoSync.SyncSource;

namespace MonoSync.SyncTarget
{
    public class SyncTargetSettings
    {
        public static SyncTargetSettings Default => new SyncTargetSettings
        {
            SyncTargetFactoryResolver = new SyncTargetFactoryResolver(),
            FieldDeserializerResolverFactory = new FieldSerializerResolverFactory(),
            TypeEncoder = new TypeEncoder()
        };

        public IFieldSerializerResolverFactory FieldDeserializerResolverFactory { get; set; }
        public ITypeEncoder TypeEncoder { get; set; }
        public ISyncTargetFactoryResolver SyncTargetFactoryResolver { get; set; }
    }
}