using System;

namespace MonoSync.Attributes
{
    /// <summary>
    /// Specifies that the type can be synchronized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class SynchronizableAttribute : Attribute
    {

    }
}