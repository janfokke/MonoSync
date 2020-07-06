using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using MonoSync.Collections;
using MonoSync.Utils;

namespace MonoSync.SyncTargetObjects
{
    public class ObservableHashSetTarget<TItem> : SyncTarget
    {
        public new ObservableHashSet<TItem> BaseObject => (ObservableHashSet<TItem>)base.BaseObject;

        private readonly Clock _clock;
        private readonly SyncTargetRoot _syncTargetRoot;
        private readonly IFieldSerializer _itemSerializer;
        
        private readonly Stack<TargetCommand> _leadingTargetCommands = new Stack<TargetCommand>();
        private readonly List<ISourceCommand> _sourceCommands = new List<ISourceCommand>();
        private readonly Stack<TargetCommand> _targetCommands = new Stack<TargetCommand>();

        private bool _subscribedToEndRead;

        /// <summary>
        ///     To avoid sync interfering with <see cref="INotifyCollectionChanged" />
        /// </summary>
        private bool _changing;

        public ObservableHashSetTarget(int referenceId, Type baseType, ExtendedBinaryReader reader,
            SyncTargetRoot syncTargetRoot, IFieldSerializerResolver fieldDeserializerResolver) : base(referenceId)
        {
            _syncTargetRoot = syncTargetRoot;
            _clock = _syncTargetRoot.Clock;

            base.BaseObject = Activator.CreateInstance(baseType);
            BaseObject.CollectionChanged += OnCollectionChanged;

            _itemSerializer = fieldDeserializerResolver.ResolveSerializer(typeof(TItem));
            
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
                    _targetCommands.Push(new TargetAddCommand(_clock.OwnTick, (TItem) e.NewItems[0]));
                    break;
                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException();
                case NotifyCollectionChangedAction.Remove:
                {
                    var oldItem = (TItem) e.OldItems[0];
                    _targetCommands.Push(new TargetRemoveCommand(_clock.OwnTick, oldItem));
                    break;
                }
            }
            SubscribeToEndRead();
        }

        public override void Dispose()
        {
            BaseObject.CollectionChanged -= OnCollectionChanged;
            UnSubscribeToEndRead();
        }

        private void SubscribeToEndRead()
        {
            if (_subscribedToEndRead == false)
            {
                _subscribedToEndRead = true;
                _syncTargetRoot.EndRead += EndRead;
            }
        }

        private void UnSubscribeToEndRead()
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
                        _sourceCommands.Add(new SourceAddCommand(reader, _itemSerializer));
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        _sourceCommands.Add(new SourceReplaceCommand(reader, _itemSerializer));
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        _sourceCommands.Add(new SourceClearCommand(reader, _itemSerializer));
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        _sourceCommands.Add(new SourceRemoveCommand(reader, _itemSerializer));
                        break;
                }
            }
        }

        public void EndRead(object o, EventArgs e)
        {
            _changing = true;

            UndoTargetCommands();
            PerformSynchronizationCommands();

            if(_leadingTargetCommands.Count > 0)
            {
                RedoLeadingTargetCommands();
            }
            else
            {
                UnSubscribeToEndRead();
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

            public abstract void Rollback(ObservableHashSet<TItem> target);

            /// <param name="target"></param>
            /// <returns>True if perform succeeded.</returns>
            public abstract void Perform(ObservableHashSet<TItem> target);
        }

        private class TargetRemoveCommand : TargetCommand
        {
            private readonly TItem _item;

            public TargetRemoveCommand(int tick, TItem item) : base(tick)
            {
                _item = item;
            }

            public override void Rollback(ObservableHashSet<TItem> target)
            {
                target.Add(_item);
            }

            public override void Perform(ObservableHashSet<TItem> target)
            {
                target.Remove(_item);
            }
        }

        private class TargetAddCommand : TargetCommand
        {
            private readonly TItem _addedItem;
            private bool _added;

            public TargetAddCommand(int tick, TItem addedItem) : base(tick)
            {
                _addedItem = addedItem;
            }

            public override void Rollback(ObservableHashSet<TItem> target)
            {
                target.Remove(_addedItem);
            }

            public override void Perform(ObservableHashSet<TItem> target)
            {
                target.Add(_addedItem);
            }
        }

        #endregion

        #region SourceCommands

        private interface ISourceCommand
        {
            void Perform(ObservableHashSet<TItem> target);
        }

        private class SourceAddCommand : ISourceCommand
        {
            private TItem _key;
            private bool _keyResolved;

            public SourceAddCommand(ExtendedBinaryReader reader, IFieldSerializer keySerializer)
            {
                keySerializer.Read(reader, synchronizedValue =>
                {
                    _key = (TItem) synchronizedValue;
                    _keyResolved = true;
                });
            }

            public void Perform(ObservableHashSet<TItem> target)
            {
                if (_keyResolved)
                {
                    target.Add(_key);
                }
            }
        }

        private class SourceReplaceCommand : ISourceCommand
        {
            private TItem _key;
            private bool _keyResolved;

            public SourceReplaceCommand(ExtendedBinaryReader reader, IFieldSerializer keyDeserializer)
            {
                keyDeserializer.Read(reader, synchronizedValue =>
                {
                    _key = (TItem) synchronizedValue;
                    _keyResolved = true;
                });
            }

            public void Perform(ObservableHashSet<TItem> target)
            {
                if (_keyResolved)
                {
                    target.Add(_key);
                }
            }
        }

        private class SourceRemoveCommand : ISourceCommand
        {
            private TItem _key;
            private bool _keyResolved;

            public SourceRemoveCommand(ExtendedBinaryReader reader, IFieldSerializer keyDeserializer)
            {
                keyDeserializer.Read(reader, synchronizedValue =>
                {
                    _key = (TItem) synchronizedValue;
                    _keyResolved = true;
                });
            }

            public void Perform(ObservableHashSet<TItem> target)
            {
                if (_keyResolved)
                {
                    if (target.Contains(_key))
                    {
                        target.Remove(_key);
                    }
                }
            }
        }

        private class SourceClearCommand : ISourceCommand
        {
            private readonly List<TItem> _items = new List<TItem>();

            public SourceClearCommand(ExtendedBinaryReader reader, IFieldSerializer itemDeserializer)
            {
                var count = reader.Read7BitEncodedInt();

                for (var i = 0; i < count; i++)
                {
                    

                    itemDeserializer.Read(reader, synchronizedValue =>
                    {
                        _items.Add((TItem) synchronizedValue);
                    });
                    
                    
                }
            }

            public void Perform(ObservableHashSet<TItem> target)
            {
                foreach (TItem item in _items)
                {
                    target.Add(item);
                }
            }
        }

        #endregion
    }
}