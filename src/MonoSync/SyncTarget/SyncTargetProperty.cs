using System;
using System.ComponentModel;
using System.Reflection;
using MonoSync.Attributes;
using MonoSync.Exceptions;
using MonoSync.PropertyStates;
using MonoSync.Utils;

namespace MonoSync
{
    public class SyncTargetProperty : IDisposable
    {
        private readonly IFieldSerializer _fieldSerializer;
        private readonly Action<object> _setter;
        private readonly SyncTargetRoot _syncTargetRoot;

        private bool _changing;

        private ISyncTargetPropertyState _state;
        private SynchronizationBehaviour _synchronizationBehaviour;
        private object _synchronizedValue;

        internal object Property
        {
            set
            {
                CheckSetter();
                _changing = true;
                _setter(value);
                _changing = false;
            }
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
                        _state = new InterpolationState(this, _syncTargetRoot, _fieldSerializer);
                        CheckSetter();
                        break;
                    case SynchronizationBehaviour.HighestTick:
                        _state = new LatestTickState(this, _syncTargetRoot);
                        CheckSetter();
                        break;
                    case SynchronizationBehaviour.TakeSynchronized:
                        _state = new TakeSynchronizedState(this);
                        CheckSetter();
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

        public SyncTargetProperty(int index,
            Action<object> setter,
            PropertyInfo propertyInfo,
            SyncTargetRoot syncTargetRoot,
            IFieldSerializer fieldSerializer)
        {
            _setter = setter;
            _syncTargetRoot = syncTargetRoot;
            _fieldSerializer = fieldSerializer;
            _state = IgnoreState.Instance;
        }

        public void Dispose()
        {
            _state?.Dispose();
        }

        private void CheckSetter()
        {
            if (_setter == null)
            {
                throw new SetterNotFoundException();
            }
        }

        internal void ReadChanges(ExtendedBinaryReader reader)
        {
            _fieldSerializer.Read(reader, value =>
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

            TickWhenDirty = _syncTargetRoot.OwnTick;
        }
    }
}