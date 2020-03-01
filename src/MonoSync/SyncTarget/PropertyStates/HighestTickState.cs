using System;

namespace MonoSync.PropertyStates
{
    internal class HighestTickState : ISyncTargetPropertyState
    {
        private readonly SyncTargetProperty _syncTargetProperty;
        private readonly SyncTargetRoot _syncTargetRoot;
        
        private object _synchronizedValue;
        private bool _subscribedToEndRead;

        public HighestTickState(SyncTargetProperty syncTargetProperty, SyncTargetRoot syncTargetRoot)
        {
            _syncTargetProperty = syncTargetProperty;
            _syncTargetRoot = syncTargetRoot;
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
            _synchronizedValue = value;
            SubscribeToEndRead();
        }

        private void SyncTargetRootOnEndRead(object sender, EventArgs e)
        {
            if (_syncTargetRoot.Clock.OtherTick > _syncTargetProperty.TickWhenDirty)
            {
                _syncTargetProperty.Property = _synchronizedValue;
                UnSubscribeToEndRead();
            }
        }

        private void SubscribeToEndRead()
        {
            if (_subscribedToEndRead == false)
            {
                _subscribedToEndRead = true;
                _syncTargetRoot.EndRead += SyncTargetRootOnEndRead;
            }
        }

        private void UnSubscribeToEndRead()
        {
            if (_subscribedToEndRead)
            {
                _subscribedToEndRead = false;
                _syncTargetRoot.EndRead -= SyncTargetRootOnEndRead;
            }
        }
    }
}