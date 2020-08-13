using System;

namespace MonoSync.Attributes
{
    /// <summary>
    ///     <see cref="SynchronizationDependencyAttribute" /> is used for constructor parameters that should be resolved with <see cref="SyncTargetSettings.ServiceProvider"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class SynchronizationDependencyAttribute : Attribute
    {

    }
}