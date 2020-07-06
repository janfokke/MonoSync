using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using MonoSync.Collections;
using MonoSync.Utils;

namespace MonoSync.SyncTargetObjects
{
    public class ObservableDictionaryTarget<TKey, TValue> : SyncTarget
    {
        public new ObservableDictionary<TKey, TValue> BaseObject => (ObservableDictionary<TKey, TValue>)base.BaseObject;

        private readonly Clock _clock;
        private readonly SyncTargetRoot _syncTargetRoot;
        private readonly IFieldSerializer _keySerializer;
        private readonly IFieldSerializer _valueSerializer;

        private readonly Stack<TargetCommand> _leadingTargetCommands = new Stack<TargetCommand>();
        private readonly List<ISourceCommand> _sourceCommands = new List<ISourceCommand>();
        private readonly Stack<TargetCommand> _targetCommands = new Stack<TargetCommand>();

        private bool _subscribedToEndRead;

        /// <summary>
        ///     To avoid sync interfering with <see cref="INotifyCollectionChanged" />
        /// </summary>
        private bool _changing;

        public ObservableDictionaryTarget(int referenceId, Type baseType, ExtendedBinaryReader reader,
            SyncTargetRoot syncTargetRoot, IFieldSerializerResolver fieldDeserializerResolver) : base(referenceId)
        {
            _syncTargetRoot = syncTargetRoot;
            _clock = _syncTargetRoot.Clock;

            base.BaseObject = Activator.CreateInstance(baseType);
            BaseObject.CollectionChanged += OnCollectionChanged;

            _keySerializer = fieldDeserializerResolver.ResolveSerializer(typeof(TKey));
            _valueSerializer = fieldDeserializerResolver.ResolveSerializer(typeof(TValue));

            Read(reader);
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_changing)
            {
                return;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    _targetCommands.Push(new TargetAddCommand(_clock.OwnTick, (KeyValuePair<TKey, TValue>) e.NewItems[0]));
                    break;
                case NotifyCollectionChangedAction.Replace:
                {
                    var oldValue = (KeyValuePair<TKey, TValue>) e.OldItems[0];
                    var newValue = (KeyValuePair<TKey, TValue>) e.NewItems[0];
                    _targetCommands.Push(new TargetReplaceCommands(_clock.OwnTick, oldValue.Key, oldValue.Value, newValue.Value));
                    break;
                }
                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException(
                        $"{nameof(ObservableDictionaryTarget<TKey, TValue>)} does not support Clearing");
                case NotifyCollectionChangedAction.Remove:
                {
                    var oldItem = (KeyValuePair<TKey, TValue>) e.OldItems[0];
                    _targetCommands.Push(new TargetRemoveCommand(_clock.OwnTick, oldItem));
                    break;
                }
            }
            SubscribeToEndRead();
        }

        public override void Dispose()
        {
            BaseObject.CollectionChanged -= OnCollectionChanged;
            UnSubscribeFromEndRead();
        }

        private void SubscribeToEndRead()
        {
            if (_subscribedToEndRead == false)
            {
                _subscribedToEndRead = true;
                _syncTargetRoot.EndRead += EndRead;
            }
        }

        private void UnSubscribeFromEndRead()
        {
            if (_subscribedToEndRead)
            {
                _subscribedToEndRead = false;
                _syncTargetRoot.EndRead -= EndRead;
            }
        }

        public sealed override void Read(ExtendedBinaryReader reader)
        {
            SubscribeToEndRead();
            var commandCount = reader.Read7BitEncodedInt();
            for (var i = 0; i < commandCount; i++)
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

            using (BaseObject.BeginMassUpdate())
            {
                UndoTargetCommands();
                PerformSynchronizationCommands();

                // If there are still leading target comments,
                // each new synchronization should check if they are later than current target tick
                if(_leadingTargetCommands.Count > 0)
                {
                    RedoLeadingTargetCommands();
                }
                else
                {
                    UnSubscribeFromEndRead();
                }
            }

            _changing = false;
        }

        private void PerformSynchronizationCommands()
        {
            foreach (ISourceCommand sourceCommand in _sourceCommands)
            {
                sourceCommand.Perform(BaseObject);
            }
            _sourceCommands.Clear();
        }

        /// <summary>
        ///     Restores commands that had a higher tick than the synchronization
        /// </summary>
        private void RedoLeadingTargetCommands()
        {
            while (_leadingTargetCommands.TryPop(out TargetCommand targetCommand))
            {
                targetCommand.Perform(BaseObject);
                _targetCommands.Push(targetCommand);
            }
        }

        private void UndoTargetCommands()
        {
            while (_targetCommands.TryPop(out TargetCommand targetCommand))
            {
                targetCommand.Rollback(BaseObject);
                // Commands with a higher tick will be restored
                if (targetCommand.Tick > _clock.OtherTick)
                {
                    _leadingTargetCommands.Push(targetCommand);
                }
            }
        }

        #region Target Commands

        private abstract class TargetCommand
        {
            public int Tick { get; }

            protected TargetCommand(int tick)
            {
                Tick = tick;
            }

            public abstract void Rollback(IDictionary<TKey, TValue> target);

            /// <param name="target"></param>
            /// <returns>True if perform succeeded.</returns>
            public abstract void Perform(IDictionary<TKey, TValue> target);
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

            public override void Perform(IDictionary<TKey, TValue> target)
            {
                if (target.ContainsKey(_item.Key))
                {
                    target.Remove(_item.Key);
                }
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

            public override void Perform(IDictionary<TKey, TValue> target)
            {
                target.TryAdd(_addedItem.Key, _addedItem.Value);
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

            public override void Perform(IDictionary<TKey, TValue> target)
            {
                if (target.TryGetValue(_key, out TValue currentValue))
                {
                    _oldValue = currentValue;
                    target[_key] = _newValue;
                }
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

            public SourceAddCommand(ExtendedBinaryReader reader, IFieldSerializer keySerializer, IFieldSerializer valueSerializer)
            {
                keySerializer.Read(reader, synchronizationCallback =>
                {
                    _key = (TKey) synchronizationCallback;
                    _keyResolved = true;
                });

                valueSerializer.Read(reader, synchronizationCallback =>
                {
                    _value = (TValue) synchronizationCallback;
                    _valueResolved = true;
                });
            }

            public void Perform(IDictionary<TKey, TValue> target)
            {
                if (_keyResolved && _valueResolved)
                {
                    target.Add(_key, _value);
                }
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
                keyDeserializer.Read(reader, synchronizationCallback =>
                {
                    _key = (TKey) synchronizationCallback;
                    _keyResolved = true;
                });

                valueDeserializer.Read(reader, synchronizationCallback =>
                {
                    _value = (TValue) synchronizationCallback;
                    _valueResolved = true;
                });
            }

            public void Perform(IDictionary<TKey, TValue> target)
            {
                if (_keyResolved && _valueResolved)
                {
                    target[_key] = _value;
                }
            }
        }

        private class SourceRemoveCommand : ISourceCommand
        {
            private TKey _key;
            private bool _keyResolved;

            public SourceRemoveCommand(ExtendedBinaryReader reader, IFieldSerializer keyDeserializer)
            {
                keyDeserializer.Read(reader, synchronizationCallback =>
                {
                    _key = (TKey) synchronizationCallback;
                    _keyResolved = true;
                });
            }

            public void Perform(IDictionary<TKey, TValue> target)
            {
                if (_keyResolved)
                {
                    if (target.ContainsKey(_key))
                    {
                        target.Remove(_key);
                    }
                }
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

                    keyDeserializer.Read(reader, synchronizationCallback => { keyValuePair.Key = (TKey) synchronizationCallback; });
                    valueDeserializer.Read(reader, synchronizationCallback => { keyValuePair.Value = (TValue) synchronizationCallback; });

                    _keyValuePairs.Add(keyValuePair);
                }
            }

            public void Perform(IDictionary<TKey, TValue> target)
            {
                foreach (BoxedKeyValuePair keyValuePair in _keyValuePairs)
                {
                    target.Add(keyValuePair.Key, keyValuePair.Value);
                }
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