namespace MonoSync.SyncTarget.PropertyStates
{
    internal class TakeSynchronizedState : ISyncTargetPropertyState
    {
        private readonly SyncTargetProperty _syncTargetProperty;

        public TakeSynchronizedState(SyncTargetProperty syncTargetProperty)
        {
            _syncTargetProperty = syncTargetProperty;
        }

        public void Dispose()
        {
            // Ignore
        }

        public void HandleRead(object value)
        {
            _syncTargetProperty.Property = value;
        }
    }
}