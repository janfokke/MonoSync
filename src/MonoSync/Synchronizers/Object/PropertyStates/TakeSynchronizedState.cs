namespace MonoSync.Synchronizers.PropertyStates
{
    internal class TakeSynchronizedState : ISyncTargetPropertyState
    {
        private readonly SynchronizableTargetMember _synchronizableTargetMember;

        public TakeSynchronizedState(SynchronizableTargetMember synchronizableTargetMember)
        {
            _synchronizableTargetMember = synchronizableTargetMember;
        }

        public void Dispose()
        {
            // Manual
        }

        public void HandleRead(object value)
        {
            _synchronizableTargetMember.Value = value;
        }
    }
}