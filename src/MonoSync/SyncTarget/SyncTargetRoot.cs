using System;
using System.IO;
using MonoSync.SyncSource;
using MonoSync.Utils;

namespace MonoSync.SyncTarget
{
    public class SyncTargetRoot<T> : SyncTargetRoot where T : class
    {
        public SyncTargetRoot(byte[] initialFullSynchronization, SyncTargetSettings settings) : base(
            initialFullSynchronization, settings)
        {
        }

        public new T Root => base.Root as T;
    }

    public class SyncTargetRoot
    {
        private readonly IFieldSerializerResolver _fieldDeserializerResolver;

        public readonly object Root;

        public SyncTargetRoot(byte[] initialFullSynchronization, SyncTargetSettings settings)
        {
            Settings = settings;
            _fieldDeserializerResolver = settings.FieldDeserializerResolverFactory.Create(ReferencePool);
            Read(initialFullSynchronization);

            // SyncObject 1 is always root object.
            ReferencePool.TryGetSyncByIdentifier(1, out SyncTarget syncTargetObject);
            Root = syncTargetObject.BaseObject;
        }

        public Clock Clock { get; } = new Clock();

        internal ReferencePool<SyncTarget> ReferencePool { get; } = new ReferencePool<SyncTarget>();

        public int OwnTick => Clock.OwnTick;
        public int OtherTick => Clock.OtherTick;

        public SyncTargetSettings Settings { get; }

        /// <summary>
        /// The amount of tick between synchronizations
        /// </summary>
        public int SendRate { get; set; } = 15;

        public void Read(byte[] data)
        {
            OnBeginRead();

            using var memoryStream = new MemoryStream(data);
            using var reader = new ExtendedBinaryReader(memoryStream);

            Clock.OtherTick = reader.Read7BitEncodedInt();

            ReferencePool.RemoveReferences(ReadRemovedReferencesIds(reader));

            ReadAddedAndChangedReferences(reader);

            OnEndRead();
        }

        private int[] ReadRemovedReferencesIds(ExtendedBinaryReader reader)
        {
            int count = reader.Read7BitEncodedInt();
            var removedReferencesIds = new int[count];
            for (var i = 0; i < count; i++)
            {
                removedReferencesIds[i] = reader.Read7BitEncodedInt();
            }

            return removedReferencesIds;
        }

        private void ReadAddedAndChangedReferences(ExtendedBinaryReader reader)
        {
            int count = reader.Read7BitEncodedInt();

            for (var i = 0; i < count; i++)
            {
                int referenceId = reader.Read7BitEncodedInt();

                if (ReferencePool.TryGetSyncByIdentifier(referenceId, out SyncTarget syncTargetObject))
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
                    ReferencePool.AddSyncObject(referenceId, syncTargetObject);
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