using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using MonoSync.Collections;
using MonoSync.Utils;

namespace MonoSync.SyncSourceObjects
{
    public class ObservableHashSetSource<TKey> : SyncSource
    {
        private readonly List<Command> _commands = new List<Command>();

        private readonly IFieldSerializer _keySerializer;

        /// <summary>
        ///     Commands don't need to be tracked if the target dictionary is cleared.
        ///     Because after clear the command will always be a full Reset
        /// </summary>
        private bool _commandsInvalidated;

        public ObservableHashSet<TKey> BaseObject => (ObservableHashSet<TKey>) Reference;

        public ObservableHashSetSource(SyncSourceRoot syncSourceRoot, int referenceId,
            ObservableHashSet<TKey> reference,
            IFieldSerializerResolver fieldSerializerResolver) : base(syncSourceRoot, referenceId, reference)
        {
            _keySerializer = fieldSerializerResolver.ResolveSerializer(typeof(TKey));
            BaseObject.CollectionChanged += OnCollectionChanged;
            AddReferences();
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            List<TKey> oldItems = e.OldItems?.Cast<TKey>().ToList();
            List<TKey> newItems = e.NewItems?.Cast<TKey>().ToList();

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                AddReferences();
            }
            else if (newItems != null)
            {
                foreach (TKey newItem in newItems)
                {
                    AddReference(newItem);
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

        private void AddReferences()
        {
            foreach (TKey item in BaseObject)
            {
                AddReference(item);
            }
        }

        private void AddReference(TKey item)
        {
            if (typeof(TKey).IsValueType == false)
            {
                if (item != null)
                {
                    SyncSourceRoot.TrackObject(item);
                }
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
                    command.Write(binaryWriter, _keySerializer);
                }
                _commands.Clear();
            }
        }

        public override void WriteFull(ExtendedBinaryWriter binaryWriter)
        {
            const int commandCount = 1;
            binaryWriter.Write7BitEncodedInt(commandCount);
            Command resetCommand = Command.FromReset(BaseObject.ToList());
            resetCommand.Write(binaryWriter, _keySerializer);
            _commandsInvalidated = false;
        }

        /// <summary>
        ///     Actions on the underlying dictionary are synchronized as commands.
        /// </summary>
        public class Command
        {
            private readonly NotifyCollectionChangedAction _action;
            private readonly IList<TKey> _items;

            private Command(NotifyCollectionChangedAction action, IList<TKey> items)
            {
                _items = items;
                _action = action;
            }

            public void Write(ExtendedBinaryWriter writer, IFieldSerializer keySerializer)
            {
                writer.Write((byte) _action);

                switch (_action)
                {
                    case NotifyCollectionChangedAction.Add:
                        keySerializer.Write(_items[0], writer);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        keySerializer.Write(_items[0], writer);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        keySerializer.Write(_items[0], writer);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                    {
                        writer.Write7BitEncodedInt(_items.Count);
                        foreach (TKey item in _items)
                        {
                            keySerializer.Write(item, writer);
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
                IList<TKey> newItems)
            {
                return new Command(NotifyCollectionChangedAction.Reset, newItems);
            }

            /// <summary>
            ///     If an item in the underlying <see cref="ObservableDictionary{TKey,TValue}" /> is replaced
            /// </summary>
            public static Command FromReplace(TKey replacement)
            {
                return new Command(NotifyCollectionChangedAction.Replace,
                    new List<TKey> {replacement});
            }

            /// <summary>
            ///     If an item is added to the underlying <see cref="ObservableDictionary{TKey,TValue}" />
            /// </summary>
            public static Command FromAdd(TKey newItem)
            {
                return new Command(NotifyCollectionChangedAction.Add,
                    new List<TKey> {newItem});
            }

            /// <summary>
            ///     If an item is remove from the underlying Dictionary with <see cref="ObservableDictionary{TKey,TValue}.Remove" />
            /// </summary>
            /// <param name="removedItem"></param>
            /// <returns></returns>
            public static Command FromRemove(TKey removedItem)
            {
                return new Command(NotifyCollectionChangedAction.Remove,
                    new List<TKey> {removedItem});
            }
        }
    }
}