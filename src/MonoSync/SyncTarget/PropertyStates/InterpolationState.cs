using System;

namespace MonoSync.PropertyStates
{
    internal class InterpolationState : ISyncTargetPropertyState
    {
        private readonly IFieldSerializer _fieldSerializer;
        private readonly SyncTargetProperty _syncTargetProperty;
        private readonly SyncTargetRoot _syncTargetRoot;
        private int _interpolatingStartTick;
        private bool _subscribedToEndRead;

        private object _interpolationSource;
        private object _interpolationTarget;

        public bool Interpolating { get; private set; }

        public InterpolationState(SyncTargetProperty syncTargetProperty, SyncTargetRoot syncTargetRoot,
            IFieldSerializer fieldSerializer)
        {
            _syncTargetProperty = syncTargetProperty;
            _syncTargetRoot = syncTargetRoot;
            _fieldSerializer = fieldSerializer;
            _syncTargetProperty.Dirty += SyncTargetPropertyOnDirty;
        }

        public void HandleRead(object value)
        {
            _interpolationTarget = value;
            SubscribeToEndRead();
        }

        public void Dispose()
        {
            _syncTargetProperty.Dirty -= SyncTargetPropertyOnDirty;
            UnSubscribeToEndRead();
            EndInterpolate();
        }

        private void SyncTargetPropertyOnDirty(object sender, EventArgs e)
        {
            if (Interpolating == false)
            {
                SubscribeToEndRead();
            }
        }

        private void SyncTargetRootOnUpdated(object sender, EventArgs e)
        {
            var interpolationFactor = Math.Min(1f,
                (_syncTargetRoot.Clock.OwnTick - _interpolatingStartTick) / (float) _syncTargetRoot.UpdateRate);
            _syncTargetProperty.Property = _fieldSerializer.Interpolate(
                _interpolationSource,
                _interpolationTarget,
                interpolationFactor);

            //Done interpolating
            if (interpolationFactor >= 1f)
            {
                EndInterpolate();
            }
        }

        private void BeginInterpolate()
        {
            if (Interpolating == false)
            {
                Interpolating = true;
                _syncTargetRoot.Updated += SyncTargetRootOnUpdated;
            }
        }

        private void EndInterpolate()
        {
            if (Interpolating)
            {
                Interpolating = false;
                _syncTargetRoot.Updated -= SyncTargetRootOnUpdated;
            }
        }

        private void SyncTargetRootOnEndRead(object sender, EventArgs e)
        {
            UnSubscribeToEndRead();

            // Previous interpolation is still running
            _interpolatingStartTick = _syncTargetRoot.Clock.OwnTick;
            _interpolationSource = _syncTargetProperty.Property;

            if (_interpolationSource == null || _interpolationTarget == null)
            {
                // Quick set
                _syncTargetProperty.Property = _interpolationTarget;
            }
            else
            {
                BeginInterpolate();
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