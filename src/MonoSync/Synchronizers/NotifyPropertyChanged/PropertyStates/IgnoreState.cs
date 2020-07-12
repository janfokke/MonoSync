namespace MonoSync.Synchronizers.PropertyStates
{
    /// <summary>
    ///     ignores synchronization
    /// </summary>
    internal class IgnoreState : ISyncTargetPropertyState
    {
        public static IgnoreState Instance { get; } = new IgnoreState();

        public void Dispose()
        {
        }

        public void HandleRead(object reader)
        {
        }
    }
}