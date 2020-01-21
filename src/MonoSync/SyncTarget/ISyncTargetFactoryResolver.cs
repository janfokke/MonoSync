using System;

namespace MonoSync.SyncTarget
{
    public interface ISyncTargetFactoryResolver
    {
        ISyncTargetFactory FindMatchingSyncTargetObjectFactory(Type baseType);
    }
}