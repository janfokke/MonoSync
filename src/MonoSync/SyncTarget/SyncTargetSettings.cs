using System;
using MonoSync.SyncTargetObjects;

namespace MonoSync
{
    public class SyncTargetSettings
    {
        public static SyncTargetSettings Default => new SyncTargetSettings
        {
            SyncTargetFactoryResolver = new SyncTargetFactoryResolver(),
            TargetFieldDeserializerResolverFactory = new TargetFieldSerializerResolverFactory(),
            TypeEncoder = new TypeEncoder()
        };

        public ITargetFieldSerializerResolverFactory TargetFieldDeserializerResolverFactory { get; set; }
        public ITypeEncoder TypeEncoder { get; set; }
        public ISyncTargetFactoryResolver SyncTargetFactoryResolver { get; set; }
        public IServiceProvider ServiceProvider { get; set; }
    }
}