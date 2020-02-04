using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using MonoSync.Collections;
using MonoSync.Utils;

namespace MonoSync.SyncSource.SyncSourceObjects
{
    public class ObservableDictionarySyncSource<TKey, TValue> : SyncSource
    {
        private readonly List<Command> _commands = new List<Command>();

        private readonly IFieldSerializer _keySerializer;

        private readonly Dictionary<object, int> _trackedReferences = new Dictionary<object, int>();
        private readonly IFieldSerializer _valueSerializer;

        /// <summary>
        ///     Commands don't need to be tracked if the target dictionary is cleared.
        ///     Because after clear the command will always be a full Reset
        /// </summary>
        private bool _commandsInvalidated;

        public ObservableDictionarySyncSource(SyncSourceRoot syncSourceRoot, int referenceId,
            ObservableDictionary<TKey, TValue> baseObject,
            IFieldSerializerResolver fieldSerializerResolver) : base(syncSourceRoot, referenceId, baseObject)
        {
            _keySerializer = fieldSerializerResolver.FindMatchingSerializer(typeof(TKey));
            _valueSerializer = fieldSerializerResolver.FindMatchingSerializer(typeof(TValue));
            BaseObject.CollectionChanged += OnCollectionChanged;
            AddNewItemReferences(BaseObject.ToList());
        }

        public new ObservableDictionary<TKey, TValue> BaseObject =>
            (ObservableDictionary<TKey, TValue>) base.BaseObject;

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            List<KeyValuePair<TKey, TValue>> oldItems = e.OldItems?.Cast<KeyValuePair<TKey, TValue>>().ToList();
            List<KeyValuePair<TKey, TValue>> newItems = e.NewItems?.Cast<KeyValuePair<TKey, TValue>>().ToList();

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                RemoveAllReference();
            }
            else
            {
                if (oldItems != null) RemoveOldItemReferences(oldItems);

                if (newItems != null) AddNewItemReferences(newItems);
            }

            if (_commandsInvalidated) return;

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

        private void RemoveOldItemReferences(IEnumerable<KeyValuePair<TKey, TValue>> oldItems)
        {
            foreach (KeyValuePair<TKey, TValue> oldItem in oldItems)
            {
                if (typeof(TKey).IsValueType == false) RemoveReference(oldItem.Key);

                if (typeof(TValue).IsValueType == false) RemoveReference(oldItem.Value);
            }
        }

        private void AddNewItemReferences(IEnumerable<KeyValuePair<TKey, TValue>> newItems)
        {
            foreach (KeyValuePair<TKey, TValue> newItem in newItems)
            {
                if (typeof(TKey).IsValueType == false) AddReference(newItem.Key);

                if (typeof(TValue).IsValueType == false) AddReference(newItem.Value);
            }
        }

        public override void Dispose()
        {
            BaseObject.CollectionChanged -= OnCollectionChanged;
        }

        public override IEnumerable<object> GetReferences()
        {
            // Making sure to collect TKeys and TValues if they are reference types
            var shouldAddKeys = typeof(TKey).IsValueType == false;
            var shouldAddValues = typeof(TValue).IsValueType == false;

            if (!shouldAddKeys && !shouldAddValues) yield break;

            foreach (KeyValuePair<TKey, TValue> item in BaseObject)
            {
                if (shouldAddKeys) yield return item.Key;

                if (shouldAddValues) yield return item.Value;
            }
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
                foreach (Command command in _commands) command.Write(binaryWriter, _keySerializer, _valueSerializer);

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
                        keySerializer.Serialize(_items[0].Key, writer);
                        valueSerializer.Serialize(_items[0].Value, writer);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        keySerializer.Serialize(_items[0].Key, writer);
                        valueSerializer.Serialize(_items[0].Value, writer);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        keySerializer.Serialize(_items[0].Key, writer);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                    {
                        writer.Write7BitEncodedInt(_items.Count);
                        foreach (KeyValuePair<TKey, TValue> item in _items)
                        {
                            keySerializer.Serialize(item.Key, writer);
                            valueSerializer.Serialize(item.Value, writer);
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

        /// <summary>
        ///     References from the source <see cref="ObservableDictionary{TKey,TValue}" /> are tracked,
        ///     because the <see cref="NotifyCollectionChangedAction.Reset" /> action doesn't specify the removed items
        /// </summary>
        /// <param name="reference"></param>

        #region

        private void RemoveReference(object reference)
        {
            if (reference == null) return;

            if (_trackedReferences.TryGetValue(reference, out var count))
            {
                if (--count <= 0)
                    _trackedReferences.Remove(reference);
                else
                    _trackedReferences[reference] = count;
            }
            else
            {
                throw new InvalidOperationException("removal of un-tracked reference");
            }

            SyncSourceRoot.RemoveReference(reference);
        }

        private void AddReference(object reference)
        {
            if (reference == null) return;

            if (_trackedReferences.TryGetValue(reference, out var count))
                _trackedReferences[reference] = ++count;
            else
                _trackedReferences[reference] = 1;

            SyncSourceRoot.AddReference(reference);
        }

        private void RemoveAllReference()
        {
            foreach (KeyValuePair<object, int> referenceCounter in _trackedReferences)
                for (var i = 0; i < referenceCounter.Value; i++)
                    SyncSourceRoot.RemoveReference(referenceCounter.Key);

            _trackedReferences.Clear();
        }

        #endregion
    }
}