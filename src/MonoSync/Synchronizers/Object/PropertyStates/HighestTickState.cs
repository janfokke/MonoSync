using System;

namespace MonoSync.Synchronizers.PropertyStates
{
    internal class HighestTickState : ISyncTargetPropertyState
    {
        private readonly SynchronizableTargetMember _synchronizableTargetMember;
        private readonly TargetSynchronizerRoot _targetSynchronizerRoot;

        private bool _subscribedToEndRead;
        private TimeSpan _tickWhenDirty;

        public HighestTickState(SynchronizableTargetMember synchronizableTargetMember, TargetSynchronizerRoot targetSynchronizerRoot)
        {
            _synchronizableTargetMember = synchronizableTargetMember;
            _targetSynchronizerRoot = targetSynchronizerRoot;
        }

        public void Dispose()
        {
            UnSubscribeToEndRead();
        }

        public void HandleRead(object value)
        {
            SubscribeToEndRead();
        }

        public void ValueChanged()
        {
            _tickWhenDirty = _targetSynchronizerRoot.Clock.OwnTick;
            SubscribeToEndRead();
        }

        private void TargetSynchronizerRootOnEndRead(object sender, EventArgs e)
        {
            if (_targetSynchronizerRoot.Clock.OtherTick > _tickWhenDirty)
            {
                _synchronizableTargetMember.Value = _synchronizableTargetMember.SynchronizedValue;
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