using System;

namespace MonoSync.Synchronizers.PropertyStates
{
    internal class HighestTickState : ISyncTargetPropertyState
    {
        private readonly SyncPropertyAccessor _syncPropertyAccessor;
        private readonly TargetSynchronizerRoot _targetSynchronizerRoot;

        private bool _subscribedToEndRead;

        public HighestTickState(SyncPropertyAccessor syncPropertyAccessor, TargetSynchronizerRoot targetSynchronizerRoot)
        {
            _syncPropertyAccessor = syncPropertyAccessor;
            _targetSynchronizerRoot = targetSynchronizerRoot;
            _syncPropertyAccessor.Dirty += SyncPropertyAccessorOnDirty;
        }

        private void SyncPropertyAccessorOnDirty(object sender, EventArgs e)
        {
            // The Property should be restored to the value of the source if the source's tick is higher than the property's dirty tick.
            SubscribeToEndRead();
        }

        public void Dispose()
        {
            _syncPropertyAccessor.Dirty -= SyncPropertyAccessorOnDirty;
            UnSubscribeToEndRead();
        }

        public void HandleRead(object value)
        {
            SubscribeToEndRead();
        }

        private void TargetSynchronizerRootOnEndRead(object sender, EventArgs e)
        {
            if (_targetSynchronizerRoot.Clock.OtherTick > _syncPropertyAccessor.TickWhenDirty)
            {
                _syncPropertyAccessor.Property = _syncPropertyAccessor.SynchronizedValue;
                UnSubscribeToEndRead();
            }
        }

        private void SubscribeToEndRead()
        {
            if (_subscribedToEndRead == false)
            {
                _subscribedToEndRead = true;
                _targetSynchronizerRoot.EndRead += TargetSynchronizerRootOnEndRead;
            }
        }

        private void UnSubscribeToEndRead()
        {
            if (_subscribedToEndRead)
            {
                _subscribedToEndRead = false;
                _targetSynchronizerRoot.EndRead -= TargetSynchronizerRootOnEndRead;
            }
        }
    }
}