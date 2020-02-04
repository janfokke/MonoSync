using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using MonoSync.Collections;
using MonoSync.SyncSource;
using MonoSync.Utils;

namespace MonoSync.SyncTarget.SyncTargetObjects
{
    public class ObservableDictionarySyncTarget<TKey, TValue> : SyncTarget
    {
        private readonly Clock _clock;
        private readonly IFieldSerializer _keySerializer;

        private readonly List<TargetCommand> _leadingTargetCommands = new List<TargetCommand>();

        /// <summary>
        ///     Source commands are performed after all references have been resolved.
        /// </summary>
        private readonly List<ISourceCommand> _sourceCommands = new List<ISourceCommand>();

        private readonly SyncTargetRoot _syncTargetRoot;

        private readonly Stack<TargetCommand> _targetCommands = new Stack<TargetCommand>();
        private readonly IFieldSerializer _valueSerializer;

        /// <summary>
        ///     To avoid sync interfering with <see cref="INotifyCollectionChanged" />
        /// </summary>
        private bool _changing;

        private int _synchronizationTick;

        public ObservableDictionarySyncTarget(int referenceId, Type baseType, ExtendedBinaryReader reader,
            SyncTargetRoot syncTargetRoot, IFieldSerializerResolver fieldDeserializerResolver) : base(referenceId)
        {
            _syncTargetRoot = syncTargetRoot;
            _clock = _syncTargetRoot.Clock;

            base.BaseObject = Activator.CreateInstance(baseType);
            BaseObject.CollectionChanged += OnCollectionChanged;

            _keySerializer = fieldDeserializerResolver.FindMatchingSerializer(typeof(TKey));
            _valueSerializer = fieldDeserializerResolver.FindMatchingSerializer(typeof(TValue));

            _syncTargetRoot.EndRead += EndRead;
            Read(reader);
        }

        public new ObservableDictionary<TKey, TValue> BaseObject =>
            (ObservableDictionary<TKey, TValue>) base.BaseObject;

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_changing) return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    _targetCommands.Push(new TargetAddCommand(_clock.OwnTick,
                        (KeyValuePair<TKey, TValue>) e.NewItems[0]));
                    break;
                case NotifyCollectionChangedAction.Replace:
                {
                    var oldValue = (KeyValuePair<TKey, TValue>) e.OldItems[0];
                    var newValue = (KeyValuePair<TKey, TValue>) e.NewItems[0];
                    _targetCommands.Push(new TargetReplaceCommands(_clock.OwnTick, oldValue.Key, oldValue.Value,
                        newValue.Value));
                    break;
                }
                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException(
                        $"{nameof(ObservableDictionarySyncTarget<TKey, TValue>)} does not support Clearing");
                case NotifyCollectionChangedAction.Remove:
                {
                    var oldItem = (KeyValuePair<TKey, TValue>) e.OldItems[0];
                    _targetCommands.Push(new TargetRemoveCommand(_clock.OwnTick, oldItem));
                    break;
                }
            }
        }

        public override void Dispose()
        {
            BaseObject.CollectionChanged -= OnCollectionChanged;
            _syncTargetRoot.EndRead -= EndRead;
        }

        public override IEnumerable<object> GetReferences()
        {
            var addKeys = typeof(TKey).IsValueType == false;
            var addValues = typeof(TValue).IsValueType == false;

            if (!addKeys && !addValues) yield break;

            foreach (KeyValuePair<TKey, TValue> item in BaseObject)
            {
                if (addKeys) yield return item.Key;

                if (addValues) yield return item.Value;
            }
        }

        public sealed override void Read(ExtendedBinaryReader reader)
        {
            _synchronizationTick = _clock.OtherTick;
            ReadSourceCommands(reader);

            // Command logic is handled in EndRead, because all references need to be fixed
        }

        private void ReadSourceCommands(ExtendedBinaryReader reader)
        {
            var count = reader.Read7BitEncodedInt();
            for (var i = 0; i < count; i++)
            {
                var action = (NotifyCollectionChangedAction) reader.ReadByte();
                switch (action)
                {
                    case NotifyCollectionChangedAction.Add:
                        _sourceCommands.Add(new SourceAddCommand(reader, _keySerializer, _valueSerializer));
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        _sourceCommands.Add(new SourceReplaceCommand(reader, _keySerializer, _valueSerializer));
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        _sourceCommands.Add(new SourceClearCommand(reader, _keySerializer, _valueSerializer));
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        _sourceCommands.Add(new SourceRemoveCommand(reader, _keySerializer));
                        break;
                }
            }
        }

        public void EndRead(object o, EventArgs e)
        {
            _changing = true;

            RollBackLeadingTargetCommands(_clock.OtherTick);

            PerformSynchronizationCommands();

            RestoreLeadingTargetCommands();

            _changing = false;
        }

        private void PerformSynchronizationCommands()
        {
            foreach (ISourceCommand sourceCommand in _sourceCommands) sourceCommand.Perform(BaseObject);

            _sourceCommands.Clear();
        }

        /// <summary>
        ///     Restores commands that had a higher tick than the synchronization
        /// </summary>
        private void RestoreLeadingTargetCommands()
        {
            foreach (TargetCommand leadingTargetCommand in _leadingTargetCommands)
                if (leadingTargetCommand.Perform(BaseObject))
                    _targetCommands.Push(leadingTargetCommand);

            _leadingTargetCommands.Clear();
        }

        /// <summary>
        ///     Because synchronization changes are leading, the commands with a lower <see cref="TargetCommand.Tick" /> than the
        ///     <see cref="synchronizationTick" /> are rolled back.
        /// </summary>
        /// <param name="synchronizationTick"></param>
        private void RollBackLeadingTargetCommands(int synchronizationTick)
        {
            while (_targetCommands.TryPop(out TargetCommand targetCommand))
            {
                targetCommand.Rollback(BaseObject);

                // Commands with a higher tick are restored later on
                if (targetCommand.Tick > synchronizationTick) _leadingTargetCommands.Add(targetCommand);
            }
        }

        #region Target Commands

        private abstract class TargetCommand
        {
            protected TargetCommand(int tick)
            {
                Tick = tick;
            }

            public int Tick { get; }

            public abstract void Rollback(IDictionary<TKey, TValue> target);

            /// <param name="target"></param>
            /// <returns>True if perform succeeded.</returns>
            public abstract bool Perform(IDictionary<TKey, TValue> target);
        }

        private class TargetRemoveCommand : TargetCommand
        {
            private readonly KeyValuePair<TKey, TValue> _item;

            public TargetRemoveCommand(int tick, KeyValuePair<TKey, TValue> item) : base(tick)
            {
                _item = item;
            }

            public override void Rollback(IDictionary<TKey, TValue> target)
            {
                target.Add(_item);
            }

            public override bool Perform(IDictionary<TKey, TValue> target)
            {
                if (target.ContainsKey(_item.Key))
                {
                    target.Remove(_item.Key);
                    return true;
                }

                // Item is already removed
                return false;
            }
        }

        private class TargetAddCommand : TargetCommand
        {
            private readonly KeyValuePair<TKey, TValue> _addedItem;

            public TargetAddCommand(int tick, KeyValuePair<TKey, TValue> addedItem) : base(tick)
            {
                _addedItem = addedItem;
            }

            public override void Rollback(IDictionary<TKey, TValue> target)
            {
                target.Remove(_addedItem.Key);
            }

            public override bool Perform(IDictionary<TKey, TValue> target)
            {
                if (target.ContainsKey(_addedItem.Key))
                    // Already added so no need to add again
                    return false;

                target.Add(_addedItem);
                return true;
            }
        }

        private class TargetReplaceCommands : TargetCommand
        {
            private readonly TKey _key;
            private readonly TValue _newValue;
            private TValue _oldValue;

            public TargetReplaceCommands(int tick, TKey key, TValue oldValue, TValue newValue) : base(tick)
            {
                _key = key;
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override void Rollback(IDictionary<TKey, TValue> target)
            {
                target[_key] = _oldValue;
            }

            public override bool Perform(IDictionary<TKey, TValue> target)
            {
                if (target.TryGetValue(_key, out TValue currentValue))
                {
                    _oldValue = currentValue;
                    target[_key] = _newValue;
                    return true;
                }

                // Value is doesn't exist anymore so it can't be replaced.
                return false;
            }
        }

        #endregion

        #region SourceCommands

        private interface ISourceCommand
        {
            void Perform(IDictionary<TKey, TValue> target);
        }

        private class SourceAddCommand : ISourceCommand
        {
            private TKey _key;
            private bool _keyResolved;
            private TValue _value;
            private bool _valueResolved;

            public SourceAddCommand(ExtendedBinaryReader reader, IFieldSerializer keySerializer,
                IFieldSerializer valueSerializer)
            {
                keySerializer.Deserialize(reader, fixup =>
                {
                    _key = (TKey) fixup;
                    _keyResolved = true;
                });

                valueSerializer.Deserialize(reader, fixup =>
                {
                    _value = (TValue) fixup;
                    _valueResolved = true;
                });
            }

            public void Perform(IDictionary<TKey, TValue> target)
            {
                if (_keyResolved && _valueResolved) target.Add(_key, _value);
            }
        }

        private class SourceReplaceCommand : ISourceCommand
        {
            private TKey _key;
            private bool _keyResolved;
            private TValue _value;
            private bool _valueResolved;

            public SourceReplaceCommand(ExtendedBinaryReader reader, IFieldSerializer keyDeserializer,
                IFieldSerializer valueDeserializer)
            {
                keyDeserializer.Deserialize(reader, fixup =>
                {
                    _key = (TKey) fixup;
                    _keyResolved = true;
                });

                valueDeserializer.Deserialize(reader, fixup =>
                {
                    _value = (TValue) fixup;
                    _valueResolved = true;
                });
            }

            public void Perform(IDictionary<TKey, TValue> target)
            {
                if (_keyResolved && _valueResolved) target[_key] = _value;
            }
        }

        private class SourceRemoveCommand : ISourceCommand
        {
            private TKey _key;
            private bool _keyResolved;

            public SourceRemoveCommand(ExtendedBinaryReader reader, IFieldSerializer keyDeserializer)
            {
                keyDeserializer.Deserialize(reader, fixup =>
                {
                    _key = (TKey) fixup;
                    _keyResolved = true;
                });
            }

            public void Perform(IDictionary<TKey, TValue> target)
            {
                if (_keyResolved) target.Remove(_key);
            }
        }

        private class SourceClearCommand : ISourceCommand
        {
            private readonly List<BoxedKeyValuePair> _keyValuePairs = new List<BoxedKeyValuePair>();

            public SourceClearCommand(ExtendedBinaryReader reader, IFieldSerializer keyDeserializer,
                IFieldSerializer valueDeserializer)
            {
                var count = reader.Read7BitEncodedInt();

                for (var i = 0; i < count; i++)
                {
                    var keyValuePair = new BoxedKeyValuePair();

                    keyDeserializer.Deserialize(reader, fixup => { keyValuePair.Key = (TKey) fixup; });

                    valueDeserializer.Deserialize(reader, fixup => { keyValuePair.Value = (TValue) fixup; });

                    _keyValuePairs.Add(keyValuePair);
                }
            }

            public void Perform(IDictionary<TKey, TValue> target)
            {
                foreach (BoxedKeyValuePair keyValuePair in _keyValuePairs)
                    target.Add(keyValuePair.Key, keyValuePair.Value);
            }

            private class BoxedKeyValuePair
            {
                public TKey Key;
                public TValue Value;
            }
        }

        #endregion
    }
}