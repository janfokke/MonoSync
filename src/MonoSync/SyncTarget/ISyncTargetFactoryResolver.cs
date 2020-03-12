using System;

namespace MonoSync
{
    public interface ISyncTargetFactoryResolver
    {
        ISyncTargetFactory FindMatchingSyncTargetObjectFactory(Type baseType);
    }
}