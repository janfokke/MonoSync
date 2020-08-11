namespace MonoSync.Synchronizers.PropertyStates
{
    internal class TakeSynchronizedState : ISyncTargetPropertyState
    {
        private readonly SyncPropertyAccessor _syncPropertyAccessor;

        public TakeSynchronizedState(SyncPropertyAccessor syncPropertyAccessor)
        {
            _syncPropertyAccessor = syncPropertyAccessor;
        }

        public void Dispose()
        {
            // Manual
        }

        public void HandleRead(object value)
        {
            _syncPropertyAccessor.Property = value;
        }
    }
}