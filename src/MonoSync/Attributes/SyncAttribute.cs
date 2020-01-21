using System;

namespace MonoSync.Attributes
{
    /// <summary>
    ///     Properties with this attribute will be synchronized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SyncAttribute : Attribute
    {
        public SyncAttribute(
            SynchronizationBehaviour synchronizationBehaviour = SynchronizationBehaviour.TakeSynchronized)
        {
            SynchronizationBehaviour = synchronizationBehaviour;
        }

        public SynchronizationBehaviour SynchronizationBehaviour { get; }
    }
}