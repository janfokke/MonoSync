using System;

namespace MonoSync.Synchronizers.PropertyStates
{
    internal class HighestTickState : ISyncTargetPropertyState
    {
        private readonly SynchronizableTargetMember _synchronizableTargetMember;
        private readonly TargetSynchronizerRoot _targetSynchronizerRoot;

        private bool _subscribedToEndRead;

        public HighestTickState(SynchronizableTargetMember synchronizableTargetMember, TargetSynchronizerRoot targetSynchronizerRoot)
        {
            _synchronizableTargetMember = synchronizableTargetMember;
            _targetSynchronizerRoot = targetSynchronizerRoot;
            _synchronizableTargetMember.Dirty += SynchronizableTargetMemberOnDirty;
        }

        private void SynchronizableTargetMemberOnDirty(object sender, EventArgs e)
        {
            // The Value should be restored to the value of the source if the source's tick is higher than the property's dirty tick.
            SubscribeToEndRead();
        }

        public void Dispose()
        {
            _synchronizableTargetMember.Dirty -= SynchronizableTargetMemberOnDirty;
            UnSubscribeToEndRead();
        }

        public void HandleRead(object value)
        {
            SubscribeToEndRead();
        }

        private void TargetSynchronizerRootOnEndRead(object sender, EventArgs e)
        {
            if (_targetSynchronizerRoot.Clock.OtherTick > _synchronizableTargetMember.TickWhenDirty)
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