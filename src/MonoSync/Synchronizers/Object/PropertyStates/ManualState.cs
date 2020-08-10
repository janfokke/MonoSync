namespace MonoSync.Synchronizers.PropertyStates
{
    /// <summary>
    ///     ignores synchronization
    /// </summary>
    internal class ManualState : ISyncTargetPropertyState
    {
        public static ManualState Instance { get; } = new ManualState();

        public void Dispose()
        {
        }

        public void HandleRead(object reader)
        {
        }
    }
}