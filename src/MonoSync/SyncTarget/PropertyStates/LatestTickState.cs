using System;

namespace MonoSync.SyncTarget.PropertyStates
{
    internal class LatestTickState : ISyncTargetPropertyState
    {
        private readonly SyncTargetProperty _syncTargetProperty;
        private readonly SyncTargetRoot _syncTargetRoot;
        private object _synchronizedValue;

        public LatestTickState(SyncTargetProperty syncTargetProperty, SyncTargetRoot syncTargetRoot)
        {
            _syncTargetProperty = syncTargetProperty;
            _syncTargetRoot = syncTargetRoot;
        }

        public void Dispose()
        {
            _syncTargetRoot.EndRead -= SyncTargetRootOnEndRead;
        }

        public void HandleRead(object value)
        {
            _synchronizedValue = value;
            _syncTargetRoot.EndRead += SyncTargetRootOnEndRead;
        }

        private void SyncTargetRootOnEndRead(object sender, EventArgs e)
        {
            if (_syncTargetRoot.Clock.OtherTick > _syncTargetProperty.TickWhenDirty)
            {
                _syncTargetProperty.Property = _synchronizedValue;
                _syncTargetRoot.EndRead -= SyncTargetRootOnEndRead;
            }
        }
    }
}