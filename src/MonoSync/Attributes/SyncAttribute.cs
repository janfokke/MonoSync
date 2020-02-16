using System;

namespace MonoSync.Attributes
{
    /// <summary>
    ///     Properties with this attribute will be synchronized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SyncAttribute : Attribute
    {
        public SynchronizationBehaviour SynchronizationBehaviour { get; }

        public SyncAttribute(
            SynchronizationBehaviour synchronizationBehaviour = SynchronizationBehaviour.TakeSynchronized)
        {
            SynchronizationBehaviour = synchronizationBehaviour;
        }
    }
}