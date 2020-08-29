using System;

namespace MonoSync.Synchronizers.PropertyStates
{
    internal class HighestTickInterpolationState : ISyncTargetPropertyState
    {
        private readonly ISerializer _serializer;
        private readonly SynchronizableTargetMember _synchronizableTargetMember;
        private readonly TargetSynchronizerRoot _targetSynchronizerRoot;
        private TimeSpan _interpolatingStartTick;
        private bool _subscribedToEndRead;

        private object _interpolationSource;
        private object _interpolationTarget;
        private TimeSpan _tickWhenDirty;

        public bool IsInterpolating { get; private set; }

        public HighestTickInterpolationState(SynchronizableTargetMember synchronizableTargetMember, TargetSynchronizerRoot targetSynchronizerRoot,
            ISerializer serializer)
        {
            _synchronizableTargetMember = synchronizableTargetMember;
            _targetSynchronizerRoot = targetSynchronizerRoot;
            _serializer = serializer;
        }

        public void HandleRead(object value)
        {
            _interpolationTarget = value;
            SubscribeToEndRead();
        }

        public void ValueChanged()
        {
            _tickWhenDirty = _targetSynchronizerRoot.Clock.OwnTick;
            EndInterpolate();
            if (IsInterpolating == false)
            {
                SubscribeToEndRead();
            }
        }

        public void Dispose()
        {
            UnSubscribeToEndRead();
            EndInterpolate();
        }

        private void TargetSynchronizerRootOnUpdated(object sender, EventArgs e)
        {
            var interpolationFactor = Math.Min(1f,
                (_targetSynchronizerRoot.Clock.OwnTick.TotalMilliseconds - _interpolatingStartTick.TotalMilliseconds) / (float) _targetSynchronizerRoot.UpdateRate.TotalMilliseconds);
            _synchronizableTargetMember.Value = _serializer.Interpolate(
                _interpolationSource,
                _interpolationTarget,
                (float) interpolationFactor);
            
            //Done interpolating
            if (interpolationFactor >= 1f)
            {
                EndInterpolate();
            }
        }

        private void BeginInterpolate()
        {
            if (IsInterpolating == false)
            {
                IsInterpolating = true;
                _targetSynchronizerRoot.Updated += TargetSynchronizerRootOnUpdated;
            }
        }

        private void EndInterpolate()
        {
            if (IsInterpolating)
            {
                IsInterpolating = false;
                _targetSynchronizerRoot.Updated -= TargetSynchronizerRootOnUpdated;
            }
        }

        private void TargetSynchronizerRootOnEndRead(object sender, EventArgs e)
        {
            if (_targetSynchronizerRoot.Clock.OtherTick > _tickWhenDirty)
            {
                UnSubscribeToEndRead();
                // Previous interpolation is still running
                _interpolatingStartTick = _targetSynchronizerRoot.Clock.OwnTick;
                _interpolationSource = _synchronizableTargetMember.Value;

                if (_interpolationSource == null || _interpolationTarget == null)
                {
                    // Quick set
                    _synchronizableTargetMember.Value = _interpolationTarget;
                }
                else
                {
                    BeginInterpolate();
                }
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