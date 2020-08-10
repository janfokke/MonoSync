using System;

namespace MonoSync.Synchronizers.PropertyStates
{
    internal class InterpolationState : ISyncTargetPropertyState
    {
        private readonly ISerializer _serializer;
        private readonly SyncTargetProperty _syncTargetProperty;
        private readonly TargetSynchronizerRoot _targetSynchronizerRoot;
        private int _interpolatingStartTick;
        private bool _subscribedToEndRead;

        private object _interpolationSource;
        private object _interpolationTarget;

        public bool Interpolating { get; private set; }

        public InterpolationState(SyncTargetProperty syncTargetProperty, TargetSynchronizerRoot targetSynchronizerRoot,
            ISerializer serializer)
        {
            _syncTargetProperty = syncTargetProperty;
            _targetSynchronizerRoot = targetSynchronizerRoot;
            _serializer = serializer;
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

        private void TargetSynchronizerRootOnUpdated(object sender, EventArgs e)
        {
            var interpolationFactor = Math.Min(1f,
                (_targetSynchronizerRoot.Clock.OwnTick - _interpolatingStartTick) / (float) _targetSynchronizerRoot.UpdateRate);
            _syncTargetProperty.Property = _serializer.Interpolate(
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
                _targetSynchronizerRoot.Updated += TargetSynchronizerRootOnUpdated;
            }
        }

        private void EndInterpolate()
        {
            if (Interpolating)
            {
                Interpolating = false;
                _targetSynchronizerRoot.Updated -= TargetSynchronizerRootOnUpdated;
            }
        }

        private void TargetSynchronizerRootOnEndRead(object sender, EventArgs e)
        {
            UnSubscribeToEndRead();

            // Previous interpolation is still running
            _interpolatingStartTick = _targetSynchronizerRoot.Clock.OwnTick;
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