using System;

namespace MonoSync.SyncTargetObjects
{
    public interface IDependencyResolver
    {
        object ResolveDependency(Type T);
    }
}