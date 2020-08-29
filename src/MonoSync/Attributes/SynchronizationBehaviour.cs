namespace MonoSync.Attributes
{
    public enum SynchronizationBehaviour
    {
        /// <summary>
        ///     Synchronization is done manually
        /// </summary>
        Manual,

        /// <summary>
        ///     Always takes the synchronized value
        /// </summary>
        TakeSynchronized,

        /// <summary>
        ///     Preserves the value as long as the target tick is higher than the synchronizationTick
        /// </summary>
        HighestTick,

        /// <summary>
        ///     Creates a smooth transition that lasts until the next synchronization.
        /// </summary>
        Interpolated,

        /// <summary>
        ///     Only sets value on construction.
        ///     If <see cref="SynchronizeAttribute" /> is used on a get only property, this state will also be implicitly used.
        /// </summary>
        Construction,

        /// <summary>
        /// Restores the member to the synchronized value on each update
        /// </summary>
        TakeSynchronizedOnEachUpdate,
        
        /// <summary>
        ///     Preserves the value as long as the target tick is higher than the synchronizationTick
        ///     Creates a smooth transition that lasts until the next synchronization.
        /// </summary>
        HighestTickInterpolated
    }
}