using System;
using System.ComponentModel;
using MonoSync.Attributes;
using MonoSync.SyncSource;
using MonoSync.SyncTarget.PropertyStates;
using MonoSync.Utils;

namespace MonoSync.SyncTarget
{
    public class SyncTargetProperty : SyncProperty, IDisposable
    {
        private readonly IFieldSerializer _fieldSerializer;
        private readonly Func<object> _getter;
        private readonly Action<object> _setter;
        private readonly SyncTargetRoot _syncTargetRoot;

        private bool _changing;

        private ISyncTargetPropertyState _state;
        private SynchronizationBehaviour _synchronizationBehaviour;
        private object _synchronizedValue;

        public SyncTargetProperty(int index, Action<object> setter, Func<object> getter,
            SyncTargetRoot syncTargetRoot,
            IFieldSerializer fieldSerializer) : base(index)
        {
            _setter = setter;
            _getter = getter;
            _syncTargetRoot = syncTargetRoot;
            _fieldSerializer = fieldSerializer;
            _state = IgnoreState.Instance;
        }

        internal object Property
        {
            set
            {
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
                    case SynchronizationBehaviour.ConstructOnly:
                        _state = new IgnoreState();
                        break;
                    case SynchronizationBehaviour.Interpolated:
                        _state = new InterpolationState(this, _syncTargetRoot, _fieldSerializer);
                        break;
                    case SynchronizationBehaviour.HighestTick:
                        _state = new LatestTickState(this, _syncTargetRoot);
                        break;
                    case SynchronizationBehaviour.TakeSynchronized:
                        _state = new TakeSynchronizedState(this);
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

        internal void ReadChanges(ExtendedBinaryReader reader)
        {
            _fieldSerializer.Deserialize(reader, value =>
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