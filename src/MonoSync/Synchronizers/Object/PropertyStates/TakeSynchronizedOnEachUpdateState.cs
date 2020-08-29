using System;

namespace MonoSync.Synchronizers.PropertyStates
{
    internal class TakeSynchronizedOnEachUpdateState : ISyncTargetPropertyState
    {
        private readonly SynchronizableTargetMember _synchronizableTargetMember;
        private readonly TargetSynchronizerRoot _targetSynchronizerRoot;

        public TakeSynchronizedOnEachUpdateState(SynchronizableTargetMember synchronizableTargetMember,
            TargetSynchronizerRoot targetSynchronizerRoot)
        {
            _synchronizableTargetMember = synchronizableTargetMember;
            _targetSynchronizerRoot = targetSynchronizerRoot;
            _targetSynchronizerRoot.EndRead += TargetSynchronizerRootOnEndRead;
        }

        public void Dispose()
        {
            _targetSynchronizerRoot.EndRead -= TargetSynchronizerRootOnEndRead;
        }

        private void TargetSynchronizerRootOnEndRead(object sender, EventArgs e)
        {
            _synchronizableTargetMember.Value = _synchronizableTargetMember.SynchronizedValue;
        }

        public void HandleRead(object reader)
        {
            
        }

        public void ValueChanged()
        {
            // Ignore
        }
    }
}