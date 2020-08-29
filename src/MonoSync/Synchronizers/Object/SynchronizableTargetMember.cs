using System;
using System.ComponentModel;
using System.Reflection;
using MonoSync.Attributes;
using MonoSync.Exceptions;
using MonoSync.Synchronizers.PropertyStates;
using MonoSync.Utils;

namespace MonoSync.Synchronizers
{
    public class SynchronizableTargetMember : IDisposable
    {
        private readonly object _declaringReference;
        private readonly TargetSynchronizerRoot _targetSynchronizerRoot;
        private readonly SynchronizableMember _synchronizableMember;
        private bool _changing;
        private ISyncTargetPropertyState _state;
        private SynchronizationBehaviour _synchronizationBehaviour;
        private object _synchronizedValue;

        public string Name => _synchronizableMember.MemberInfo.Name;

        public SynchronizableTargetMember(
            object declaringReference,
            SynchronizableMember synchronizableMember,
            TargetSynchronizerRoot targetSynchronizerRoot)
        {
            _synchronizableMember = synchronizableMember;
            _targetSynchronizerRoot = targetSynchronizerRoot;
            _declaringReference = declaringReference;
            _state = ManualState.Instance;
        }

        internal object Value
        {
            set
            {
                AssertHasSetter();
                _changing = true;
                _synchronizableMember.SetValue(_declaringReference, value);
                _changing = false;
            }
            get => _synchronizableMember.GetValue(_declaringReference);
        }

        /// <summary>
        ///     Last synchronized value
        /// </summary>
        public object SynchronizedValue
        {
            get => _synchronizedValue;
            private set
            {
                _synchronizedValue = value;
                Synchronized = true;
            }
        }

        /// <summary>
        ///     Used to differentiate null and unsynchronized
        /// </summary>
        public bool Synchronized { get; private set; }

        /// <summary>
        ///     Tick when underlying property changed
        /// </summary>

        public SynchronizationBehaviour DefaultSynchronizationBehaviour =>
            _synchronizableMember.SynchronizeAttribute.SynchronizationBehaviour;

        public SynchronizationBehaviour SynchronizationBehaviour
        {
            get => _synchronizationBehaviour;
            set
            {
                _synchronizationBehaviour = value;

                _state.Dispose();

                switch (value)
                {
                    case SynchronizationBehaviour.Manual:
                        _state = new ManualState();
                        break;
                    case SynchronizationBehaviour.Construction:
                        _state = new ConstructionState();
                        break;
                    case SynchronizationBehaviour.Interpolated:
                        _state = new InterpolationState(this, _targetSynchronizerRoot, _synchronizableMember.Serializer);
                        AssertHasSetter();
                        break;
                    case SynchronizationBehaviour.HighestTickInterpolated:
                        _state = new HighestTickInterpolationState(this, _targetSynchronizerRoot, _synchronizableMember.Serializer);
                        AssertHasSetter();
                        break;
                    case SynchronizationBehaviour.HighestTick:
                        _state = new HighestTickState(this, _targetSynchronizerRoot);
                        AssertHasSetter();
                        break;
                    case SynchronizationBehaviour.TakeSynchronized:
                        _state = new TakeSynchronizedState(this);
                        AssertHasSetter();
                        break;
                    case SynchronizationBehaviour.TakeSynchronizedOnEachUpdate:
                        _state = new TakeSynchronizedOnEachUpdateState(this, _targetSynchronizerRoot);
                        AssertHasSetter();
                        break;
                }
            }
        }

        public bool IsInterpolating
        {
            get
            {
                if (_state is InterpolationState interpolationState)
                {
                    return interpolationState.IsInterpolating;
                }

                return false;
            }
        }

        public void Dispose()
        {
            _state?.Dispose();
        }

        private void AssertHasSetter()
        {
            if (_synchronizableMember.CanSet == false)
            {
                throw new SetterNotAvailableException(_synchronizableMember.MemberInfo);
            }
        }

        internal void ReadChanges(ExtendedBinaryReader reader)
        {
            _synchronizableMember.Serializer.Read(reader, value =>
            {
                SynchronizedValue = value;
                _state.HandleRead(value);
            });
        }

        /// <summary>
        ///     Invoked when <see cref="INotifyPropertyChanged.PropertyChanged" /> event fires.
        /// </summary>
        internal void NotifyChanged()
        {
            if (_changing)
            {
                return;
            }
            _state.ValueChanged();
        }
    }
}