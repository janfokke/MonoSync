using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Text;
using MonoSync.Serializers;
using MonoSync.Utils;

namespace MonoSync
{
    public class TargetSynchronizerRoot<T> : TargetSynchronizerRoot where T : class
    {
        public new T Reference => base.Reference as T;

        public TargetSynchronizerRoot(byte[] initialFullSynchronization,Settings settings = null, IServiceProvider serviceProvider = null) : base(initialFullSynchronization,settings, serviceProvider)
        {

        }
    }

    public class TargetSynchronizerRoot
    {
        internal static readonly List<WeakReference<TargetSynchronizerRoot>> Instances = 
            new List<WeakReference<TargetSynchronizerRoot>>();

        public Settings Settings { get; }
        public object Reference { get; }

        private readonly TypeEncoder _typeEncoder = new TypeEncoder();

        public readonly IServiceProvider ServiceProvider;

        public Clock Clock { get; } = new Clock();

        internal TargetReferencePool ReferencePool { get; } = new TargetReferencePool();
        internal SynchronizableMemberFactory SynchronizableMemberFactory { get; }


        /// <summary>
        /// Amount of updates between reads
        /// </summary>
        public int UpdateRate { get; private set; }

        private int _updateRateCounter;

        public TargetSynchronizerRoot(byte[] initialFullSynchronization,Settings settings = null ,IServiceProvider serviceProvider = null)
        {
            Instances.Add(new WeakReference<TargetSynchronizerRoot>(this));

            Settings = settings ?? Settings.Default();
            Settings.Serializers.AddSerializer(new TargetReferenceSerializer(ReferencePool));
            SynchronizableMemberFactory = new SynchronizableMemberFactory(Settings.Serializers);

            ServiceProvider = serviceProvider;
            
            Read(initialFullSynchronization);

            // SyncObject 1 is always synchronizerRoot object.
            ReferencePool.TryGetSynchronizerByIdentifier(1, out TargetSynchronizer syncTargetObject);
            Reference = syncTargetObject.Reference;
        }

        public void Read(byte[] data)
        {
            UpdateRate = _updateRateCounter;
            _updateRateCounter = 0;

            OnBeginRead();

            using var memoryStream = new MemoryStream(data);
            using var reader = new ExtendedBinaryReader(memoryStream);

            Clock.OtherTick = reader.Read7BitEncodedInt();

            _typeEncoder.Read(reader);

            // References are removed after read
            int[] readRemovedReferencesIds = ReadRemovedReferencesIds(reader);

            ReadAddedAndChangedReferences(reader);

            ReferencePool.RemoveReferences(readRemovedReferencesIds);

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

                if (ReferencePool.TryGetSynchronizerByIdentifier(referenceId, out TargetSynchronizer targetSynchronizer))
                {
                    targetSynchronizer.Read(reader);
                }
                else
                {
                    Type referenceType = _typeEncoder.ReadType(reader);
                    ISynchronizer synchronizer = Settings.Synchronizers.FindSynchronizerByType(referenceType);
                    targetSynchronizer = synchronizer.Synchronize(this, referenceId, referenceType);
                    targetSynchronizer.Read(reader);
                    ReferencePool.AddSynchronizer(referenceId, targetSynchronizer);
                }
            }
        }

        public event EventHandler Updated;

        public event EventHandler BeginRead;

        public event EventHandler EndRead;

        public void Update()
        {
            _updateRateCounter++;

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