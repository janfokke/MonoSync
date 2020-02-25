using System;

namespace MonoSync.Attributes
{
    /// <summary>
    ///     <see cref="SyncDependencyAttribute" /> is used for constructor parameters that should be resolved with <see cref="SyncTargetRoot.DependencyResolver"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class SyncDependencyAttribute : Attribute
    {

    }
}