using System;
using System.Diagnostics;
using System.IO;

namespace MonoSync.Attributes
{
    /// <summary>
    ///     Properties with this attribute will be synchronized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SynchronizeAttribute : Attribute
    {
        public SynchronizationBehaviour SynchronizationBehaviour { get; }

        public SynchronizeAttribute(
            SynchronizationBehaviour synchronizationBehaviour = SynchronizationBehaviour.TakeSynchronized)
        {
            SynchronizationBehaviour = synchronizationBehaviour;
        }
    }
}