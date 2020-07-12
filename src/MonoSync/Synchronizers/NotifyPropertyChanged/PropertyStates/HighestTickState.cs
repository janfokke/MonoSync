using System;

namespace MonoSync.Synchronizers.PropertyStates
{
    internal class HighestTickState : ISyncTargetPropertyState
    {
        private readonly SyncTargetProperty _syncTargetProperty;
        private readonly TargetSynchronizerRoot _targetSynchronizerRoot;

        private bool _subscribedToEndRead;

        public HighestTickState(SyncTargetProperty syncTargetProperty, TargetSynchronizerRoot targetSynchronizerRoot)
        {
            _syncTargetProperty = syncTargetProperty;
            _targetSynchronizerRoot = targetSynchronizerRoot;
            _syncTargetProperty.Dirty += SyncTargetPropertyOnDirty;
        }

        private void SyncTargetPropertyOnDirty(object sender, EventArgs e)
        {
            // The Property should be restored to the value of the source if the source's tick is higher than the property's dirty tick.
            SubscribeToEndRead();
        }

        public void Dispose()
        {
            _syncTargetProperty.Dirty -= SyncTargetPropertyOnDirty;
            UnSubscribeToEndRead();
        }

        public void HandleRead(object value)
        {
            SubscribeToEndRead();
        }

        private void TargetSynchronizerRootOnEndRead(object sender, EventArgs e)
        {
            if (_targetSynchronizerRoot.Clock.OtherTick > _syncTargetProperty.TickWhenDirty)
            {
                _syncTargetProperty.Property = _syncTargetProperty.SynchronizedValue;
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