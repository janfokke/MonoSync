using System;
using System.IO;
using MonoSync.SyncTargetObjects;
using MonoSync.Utils;

namespace MonoSync
{
    public class SyncTargetRoot<T> : SyncTargetRoot where T : class
    {
        public new T Root => base.Root as T;

        public SyncTargetRoot(byte[] initialFullSynchronization, SyncTargetSettings settings) : base(
            initialFullSynchronization, settings)
        {
        }
    }

    public class SyncTargetRoot
    {
        private readonly IFieldSerializerResolver _fieldDeserializerResolver;

        public readonly object Root;

        public Clock Clock { get; } = new Clock();

        internal TargetReferencePool TargetReferencePool { get; } = new TargetReferencePool();

        public int OwnTick => Clock.OwnTick;
        public int OtherTick => Clock.OtherTick;

        public SyncTargetSettings Settings { get; }

        /// <summary>
        ///     The amount of tick between synchronizations
        /// </summary>
        public int SendRate { get; set; } = 15;

        public SyncTargetRoot(byte[] initialFullSynchronization, SyncTargetSettings settings)
        {
            Settings = settings;
            _fieldDeserializerResolver = settings.TargetFieldDeserializerResolverFactory.Create(TargetReferencePool);
            Read(initialFullSynchronization);

            // SyncObject 1 is always root object.
            TargetReferencePool.TryGetSyncTargetByIdentifier(1, out SyncTarget syncTargetObject);
            Root = syncTargetObject.BaseObject;
        }

        public void Read(byte[] data)
        {
            OnBeginRead();

            using var memoryStream = new MemoryStream(data);
            using var reader = new ExtendedBinaryReader(memoryStream);

            Clock.OtherTick = reader.Read7BitEncodedInt();

            // References are removed after read
            int[] readRemovedReferencesIds = ReadRemovedReferencesIds(reader);

            ReadAddedAndChangedReferences(reader);

            TargetReferencePool.RemoveReferences(readRemovedReferencesIds);

            OnEndRead();
        }

        private int[] ReadRemovedReferencesIds(ExtendedBinaryReader reader)
        {
            var count = reader.Read7BitEncodedInt();
            var removedReferencesIds = new int[count];
            for (var i = 0; i < count; i++)
            {
                removedReferencesIds[i] = reader.Read7BitEncodedInt();
            }

            return removedReferencesIds;
        }

        private void ReadAddedAndChangedReferences(ExtendedBinaryReader reader)
        {
            var count = reader.Read7BitEncodedInt();

            for (var i = 0; i < count; i++)
            {
                var referenceId = reader.Read7BitEncodedInt();

                if (TargetReferencePool.TryGetSyncTargetByIdentifier(referenceId, out SyncTarget syncTargetObject))
                {
                    syncTargetObject.Read(reader);
                }
                else
                {
                    Type type = Settings.TypeEncoder.ReadType(reader);

                    ISyncTargetFactory syncTargetFactory =
                        Settings.SyncTargetFactoryResolver.FindMatchingSyncTargetObjectFactory(type);

                    syncTargetObject =
                        syncTargetFactory.Create(referenceId, type, reader, _fieldDeserializerResolver, this);
                    TargetReferencePool.AddSyncObject(referenceId, syncTargetObject);
                }
            }
        }

        public event EventHandler Updated;

        public event EventHandler BeginRead;

        public event EventHandler EndRead;

        public void Update()
        {
            Clock.Update();
            OnUpdated();
        }

        protected virtual void OnUpdated()
        {
            Updated?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnBeginRead()
        {
            BeginRead?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnEndRead()
        {
            EndRead?.Invoke(this, EventArgs.Empty);
        }
    }
}