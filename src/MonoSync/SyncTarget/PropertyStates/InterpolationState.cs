using System;
using MonoSync.SyncSource;

namespace MonoSync.SyncTarget.PropertyStates
{
    internal class InterpolationState : ISyncTargetPropertyState
    {
        private readonly IFieldSerializer _fieldSerializer;
        private readonly SyncTargetProperty _syncTargetProperty;
        private readonly SyncTargetRoot _syncTargetRoot;
        private int _interpolatingStartTick;
        private object _previousSynchronizedValue;
        private object _synchronizedValue;

        public InterpolationState(SyncTargetProperty syncTargetProperty, SyncTargetRoot syncTargetRoot,
            IFieldSerializer fieldSerializer)
        {
            _synchronizedValue = syncTargetProperty.Property;
            _syncTargetProperty = syncTargetProperty;
            _syncTargetRoot = syncTargetRoot;
            _fieldSerializer = fieldSerializer;
        }

        public bool IsInterpolating { get; set; }

        public void HandleRead(object value)
        {
            _previousSynchronizedValue = _synchronizedValue;
            _synchronizedValue = value;
            _interpolatingStartTick = _syncTargetRoot.Clock.OwnTick;
            if (
                _previousSynchronizedValue != null &&
                _synchronizedValue != null &&
                _previousSynchronizedValue != _synchronizedValue
            )
            {
                if (IsInterpolating == false)
                {
                    IsInterpolating = true;
                    _syncTargetRoot.Updated += Update;
                }
            }
        }

        public void Dispose()
        {
            if (IsInterpolating)
            {
                _syncTargetRoot.Updated -= Update;
                IsInterpolating = false;
            }
        }

        private void Update(object sender, EventArgs e)
        {
            float interpolationFactor = Math.Min(1f,
                (_syncTargetRoot.Clock.OwnTick - _interpolatingStartTick) / (float)_syncTargetRoot.SendRate);
            _syncTargetProperty.Property = _fieldSerializer.Interpolate(
                _previousSynchronizedValue,
                _synchronizedValue,
                interpolationFactor);

            //Done interpolating
            if (interpolationFactor >= 1f)
            {
                FinishInterpolation();
            }
        }

        private void FinishInterpolation()
        {
            IsInterpolating = false;
            _syncTargetRoot.Updated -= Update;
        }
    }
}