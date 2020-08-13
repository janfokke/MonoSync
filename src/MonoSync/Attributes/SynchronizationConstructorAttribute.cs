using System;

namespace MonoSync.Attributes
{
    /// <summary>
    ///     Indicates which constructor should be used for synchronization
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor)]
    public class SynchronizationConstructorAttribute : Attribute
    {
    }
}