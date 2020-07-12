using System;
using System.ComponentModel;
using System.Reflection;
using MonoSync.Attributes;
using MonoSync.Exceptions;
using MonoSync.Synchronizers.PropertyStates;
using MonoSync.Utils;

namespace MonoSync.Synchronizers
{
    public class SyncTargetProperty : IDisposable
    {
        private readonly ISerializer _serializer;
        private readonly Action<object> _setter;
        private readonly Func<object> _getter;
        private readonly TargetSynchronizerRoot _targetSynchronizerRoot;
        private readonly PropertyInfo _propertyInfo;

        private bool _changing;

        private ISyncTargetPropertyState _state;
        private SynchronizationBehaviour _synchronizationBehaviour;
        private object _synchronizedValue;

        internal event EventHandler Dirty;

        internal object Property
        {
            set
            {
                HasSetter();
                _changing = true;
                _setter(value);
                _changing = false;
            }
            get => _getter();
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
        internal int TickWhenDirty { get; private set; }

        public SynchronizationBehaviour SynchronizationBehaviour
        {
            get => _synchronizationBehaviour;
            set
            {
                _synchronizationBehaviour = value;

                _state.Dispose();

                switch (value)
                {
                    case SynchronizationBehaviour.Ignore:
                        _state = new IgnoreState();
                        break;
                    case SynchronizationBehaviour.Construction:
                        _state = new ConstructionState();
                        break;
                    case SynchronizationBehaviour.Interpolated:
                        _state = new InterpolationState(this, _targetSynchronizerRoot, _serializer);
                        HasSetter();
                        break;
                    case SynchronizationBehaviour.HighestTick:
                        _state = new HighestTickState(this, _targetSynchronizerRoot);
                        HasSetter();
                        break;
                    case SynchronizationBehaviour.TakeSynchronized:
                        _state = new TakeSynchronizedState(this);
                        HasSetter();
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
                    return interpolationState.Interpolating;
                }

                return false;
            }
        }

        public SyncTargetProperty(
            PropertyInfo propertyInfo,
            Action<object> setter,
            Func<object> getter,
            TargetSynchronizerRoot targetSynchronizerRoot,
            ISerializer serializer)
        {
            _propertyInfo = propertyInfo;
            _setter = setter;
            _getter = getter;
            _targetSynchronizerRoot = targetSynchronizerRoot;
            _serializer = serializer;
            _state = IgnoreState.Instance;
        }

        public void Dispose()
        {
            _state?.Dispose();
        }

        private void HasSetter()
        {
            if (_setter == null)
            {
                throw new SetterNotFoundException(_propertyInfo);
            }
        }

        internal void ReadChanges(ExtendedBinaryReader reader)
        {
            _serializer.Read(reader, value =>
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
            
            TickWhenDirty = _targetSynchronizerRoot.Clock.OwnTick;
            Dirty?.Invoke(this, EventArgs.Empty);
        }
    }
}