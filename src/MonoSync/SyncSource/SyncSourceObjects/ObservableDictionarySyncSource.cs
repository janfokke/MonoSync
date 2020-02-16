using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using MonoSync.Collections;
using MonoSync.Utils;

namespace MonoSync.SyncSourceObjects
{
    public class ObservableDictionarySyncSource<TKey, TValue> : SyncSource
    {
        private readonly List<Command> _commands = new List<Command>();

        private readonly IFieldSerializer _keySerializer;
        private readonly IFieldSerializer _valueSerializer;

        /// <summary>
        ///     Commands don't need to be tracked if the target dictionary is cleared.
        ///     Because after clear the command will always be a full Reset
        /// </summary>
        private bool _commandsInvalidated;

        public ObservableDictionary<TKey, TValue> BaseObject =>
            (ObservableDictionary<TKey, TValue>) Reference;

        public ObservableDictionarySyncSource(SyncSourceRoot syncSourceRoot, int referenceId,
            ObservableDictionary<TKey, TValue> reference,
            IFieldSerializerResolver fieldSerializerResolver) : base(syncSourceRoot, referenceId, reference)
        {
            _keySerializer = fieldSerializerResolver.ResolveSerializer(typeof(TKey));
            _valueSerializer = fieldSerializerResolver.ResolveSerializer(typeof(TValue));
            BaseObject.CollectionChanged += OnCollectionChanged;
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
                    SyncSourceRoot.TrackObject(newItem.Key);
                }
            }

            if (typeof(TValue).IsValueType == false)
            {
                if (newItem.Value != null)
                {
                    SyncSourceRoot.TrackObject(newItem.Value);
                }
            }
        }

        private void AddReferences()
        {
            foreach (KeyValuePair<TKey, TValue> item in BaseObject)
            {
                AddReferencesFromKeyValuePair(item);
            }
        }

        public override void Dispose()
        {
            BaseObject.CollectionChanged -= OnCollectionChanged;
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

            _commandsInvalidated = false;
        }

        public override void WriteFull(ExtendedBinaryWriter binaryWriter)
        {
            const int commandCount = 1;
            binaryWriter.Write7BitEncodedInt(commandCount);
            Command resetCommand = Command.FromReset(BaseObject.ToList());
            resetCommand.Write(binaryWriter, _keySerializer, _valueSerializer);
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

            public void Write(ExtendedBinaryWriter writer, IFieldSerializer keySerializer,
                IFieldSerializer valueSerializer)
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
            ///     If the underlying <see cref="ObservableDictionary{TKey,TValue}" /> is cleared.
            /// </summary>
            /// <param name="newItems"></param>
            /// <returns></returns>
            public static Command FromReset(
                IList<KeyValuePair<TKey, TValue>> newItems)
            {
                return new Command(NotifyCollectionChangedAction.Reset, newItems);
            }

            /// <summary>
            ///     If an item in the underlying <see cref="ObservableDictionary{TKey,TValue}" /> is replaced
            /// </summary>
            public static Command FromReplace(KeyValuePair<TKey, TValue> replacement)
            {
                return new Command(NotifyCollectionChangedAction.Replace,
                    new List<KeyValuePair<TKey, TValue>> {replacement});
            }

            /// <summary>
            ///     If an item is added to the underlying <see cref="ObservableDictionary{TKey,TValue}" />
            /// </summary>
            public static Command FromAdd(
                KeyValuePair<TKey, TValue> newItem)
            {
                return new Command(NotifyCollectionChangedAction.Add,
                    new List<KeyValuePair<TKey, TValue>> {newItem});
            }

            /// <summary>
            ///     If an item is remove from the underlying Dictionary with <see cref="ObservableDictionary{TKey,TValue}.Remove" />
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