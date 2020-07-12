using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using MonoSync.Utils;

namespace MonoSync.Synchronizers
{
    public class ObservableDictionarySourceSynchronizer<TKey, TValue> : SourceSynchronizer
    {
        private readonly List<Command> _commands = new List<Command>();

        private readonly ISerializer _keySerializer;
        private readonly ISerializer _valueSerializer;

        /// <summary>
        ///     Commands don't need to be tracked if the target dictionary is cleared.
        ///     Because after clear the command will always be a full Reset
        /// </summary>
        private bool _commandsInvalidated;

        public Collections.ObservableDictionary<TKey, TValue> Reference => (Collections.ObservableDictionary<TKey, TValue>) base.Reference;

        public ObservableDictionarySourceSynchronizer(SourceSynchronizerRoot sourceSynchronizerRoot, int referenceId, Collections.ObservableDictionary<TKey, TValue> reference) : base(sourceSynchronizerRoot, referenceId, reference)
        {
            _keySerializer = sourceSynchronizerRoot.Settings.Serializers.FindSerializerByType(typeof(TKey));
            _valueSerializer = sourceSynchronizerRoot.Settings.Serializers.FindSerializerByType(typeof(TValue));
            Reference.CollectionChanged += OnCollectionChanged;
            AddReferences();
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            List<KeyValuePair<TKey, TValue>> oldItems = e.OldItems?.Cast<KeyValuePair<TKey, TValue>>().ToList();
            List<KeyValuePair<TKey, TValue>> newItems = e.NewItems?.Cast<KeyValuePair<TKey, TValue>>().ToList();

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                AddReferences();
            }
            else if (newItems != null)
            {
                foreach (KeyValuePair<TKey, TValue> newItem in newItems)
                {
                    AddReferencesFromKeyValuePair(newItem);
                }
            }

            if (_commandsInvalidated)
            {
                return;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    _commands.Add(Command.FromAdd(newItems[0]));
                    break;
                case NotifyCollectionChangedAction.Replace:
                    // Old value is not needed for sync
                    _commands.Add(Command.FromReplace(newItems[0]));
                    break;
                case NotifyCollectionChangedAction.Reset:
                    // No need to keep old commands in case of reset
                    _commands.Clear();
                    _commandsInvalidated = true;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    _commands.Add(Command.FromRemove(oldItems[0]));
                    break;
            }

            base.MarkDirty();
        }

        private void AddReferencesFromKeyValuePair(KeyValuePair<TKey, TValue> newItem)
        {
            if (typeof(TKey).IsValueType == false)
            {
                if (newItem.Key != null)
                {
                    SourceSynchronizerRoot.Synchronize(newItem.Key);
                }
            }

            if (typeof(TValue).IsValueType == false)
            {
                if (newItem.Value != null)
                {
                    SourceSynchronizerRoot.Synchronize(newItem.Value);
                }
            }
        }

        private void AddReferences()
        {
            foreach (KeyValuePair<TKey, TValue> item in Reference)
            {
                AddReferencesFromKeyValuePair(item);
            }
        }

        public override void Dispose()
        {
            Reference.CollectionChanged -= OnCollectionChanged;
        }

        public override void WriteChanges(ExtendedBinaryWriter binaryWriter)
        {
            // A full reset if the underlying dictionary is cleared
            if (_commandsInvalidated)
            {
                WriteFull(binaryWriter);
            }
            else
            {
                binaryWriter.Write7BitEncodedInt(_commands.Count);
                foreach (Command command in _commands)
                {
                    command.Write(binaryWriter, _keySerializer, _valueSerializer);
                }
                _commands.Clear();
            }
        }

        public override void WriteFull(ExtendedBinaryWriter binaryWriter)
        {
            const int commandCount = 1;
            binaryWriter.Write7BitEncodedInt(commandCount);
            Command resetCommand = Command.FromReset(Reference.ToList());
            resetCommand.Write(binaryWriter, _keySerializer, _valueSerializer);
            _commandsInvalidated = false;
        }

        /// <summary>
        ///     Actions on the underlying dictionary are synchronized as commands.
        /// </summary>
        public class Command
        {
            private readonly NotifyCollectionChangedAction _action;
            private readonly IList<KeyValuePair<TKey, TValue>> _items;

            private Command(NotifyCollectionChangedAction action, IList<KeyValuePair<TKey, TValue>> items)
            {
                _items = items;
                _action = action;
            }

            public void Write(ExtendedBinaryWriter writer, ISerializer keySerializer,
                ISerializer valueSerializer)
            {
                writer.Write((byte) _action);

                switch (_action)
                {
                    case NotifyCollectionChangedAction.Add:
                        keySerializer.Write(_items[0].Key, writer);
                        valueSerializer.Write(_items[0].Value, writer);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        keySerializer.Write(_items[0].Key, writer);
                        valueSerializer.Write(_items[0].Value, writer);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        keySerializer.Write(_items[0].Key, writer);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                    {
                        writer.Write7BitEncodedInt(_items.Count);
                        foreach (KeyValuePair<TKey, TValue> item in _items)
                        {
                            keySerializer.Write(item.Key, writer);
                            valueSerializer.Write(item.Value, writer);
                        }
                        break;
                    }
                }
            }

            /// <summary>
            ///     If the underlying <see cref="Collections.ObservableDictionaryTargetSynchronizer{TKey,TValue}" /> is cleared.
            /// </summary>
            /// <param name="newItems"></param>
            /// <returns></returns>
            public static Command FromReset(
                IList<KeyValuePair<TKey, TValue>> newItems)
            {
                return new Command(NotifyCollectionChangedAction.Reset, newItems);
            }

            /// <summary>
            ///     If an item in the underlying <see cref="Collections.ObservableDictionaryTargetSynchronizer{TKey,TValue}" /> is replaced
            /// </summary>
            public static Command FromReplace(KeyValuePair<TKey, TValue> replacement)
            {
                return new Command(NotifyCollectionChangedAction.Replace,
                    new List<KeyValuePair<TKey, TValue>> {replacement});
            }

            /// <summary>
            ///     If an item is added to the underlying <see cref="Collections.ObservableDictionaryTargetSynchronizer{TKey,TValue}" />
            /// </summary>
            public static Command FromAdd(
                KeyValuePair<TKey, TValue> newItem)
            {
                return new Command(NotifyCollectionChangedAction.Add,
                    new List<KeyValuePair<TKey, TValue>> {newItem});
            }

            /// <summary>
            ///     If an item is remove from the underlying Dictionary with <see cref="Collections.ObservableDictionaryTargetSynchronizer{TKey,TValue}.Remove" />
            /// </summary>
            /// <param name="removedItem"></param>
            /// <returns></returns>
            public static Command FromRemove(
                KeyValuePair<TKey, TValue> removedItem)
            {
                return new Command(NotifyCollectionChangedAction.Remove,
                    new List<KeyValuePair<TKey, TValue>> {removedItem});
            }
        }
    }
}