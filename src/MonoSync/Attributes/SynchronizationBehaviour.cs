namespace MonoSync.Attributes
{
    public enum SynchronizationBehaviour
    {
        /// <summary>
        ///     Only synchronizes value on construction and ignores and further synchronizations
        /// </summary>
        Ignore,

        /// <summary>
        ///     Always overrides the target with the synchronized value
        /// </summary>
        TakeSynchronized,

        /// <summary>
        ///     Preserves the value as long as the target tick is higher than the synchronizationTick
        /// </summary>
        HighestTick,

        /// <summary>
        ///     Creates a smooth transition that lasts until the next synchronization.
        /// </summary>
        Interpolated
    }
}