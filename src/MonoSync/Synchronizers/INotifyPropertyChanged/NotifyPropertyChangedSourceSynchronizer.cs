using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using MonoSync.Utils;

namespace MonoSync.Synchronizers
{
    public class NotifyPropertyChangedSourceSynchronizer : ObjectSourceSynchronizer
    {
        private new INotifyPropertyChanged Reference => (INotifyPropertyChanged)base.Reference;
        private readonly SortedDictionary<int, SyncSourceProperty> _changedProperties = new SortedDictionary<int, SyncSourceProperty>();

        public NotifyPropertyChangedSourceSynchronizer(SourceSynchronizerRoot sourceSynchronizerRoot, int referenceId, INotifyPropertyChanged reference) : base(sourceSynchronizerRoot, referenceId, reference)
        {
            reference.PropertyChanged += SourceObjectOnPropertyChanged;
        }

        public override void WriteChanges(ExtendedBinaryWriter binaryWriter)
        {
            var changedPropertiesMask = new BitArray(SyncPropertyCollection.Length);

            // Mark changed properties bitArray
            foreach (SyncSourceProperty syncSourceProperty in _changedProperties.Values)
            {
                changedPropertiesMask[syncSourceProperty.Index] = true;
            }

            binaryWriter.Write(changedPropertiesMask);

            foreach (SyncSourceProperty sourceProperty in _changedProperties.Values)
            {
                object value = TypeAccessor[Reference, sourceProperty.Name];
                sourceProperty.Serializer.Write(value, binaryWriter);
            }
            MarkClean();
        }

        private void SourceObjectOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var propertyName = e.PropertyName;

            // Property changed event might be from a non-sync property
            if (SyncPropertyCollection.TryGetPropertyByName(e.PropertyName, out SyncSourceProperty syncSourceProperty))
            {
                object newValue = TypeAccessor[Reference, propertyName];

                if (syncSourceProperty.IsValueType == false)
                {
                    if (newValue != null)
                    {
                        SourceSynchronizerRoot.Synchronize(newValue);
                    }
                }

                _changedProperties.TryAdd(syncSourceProperty.Index, syncSourceProperty);
                if (Dirty == false)
                {
                    MarkDirty();
                }
            }
        }

        public override void WriteFull(ExtendedBinaryWriter binaryWriter)
        {
            var changedPropertiesMask = new BitArray(SyncPropertyCollection.Length, true);
            // All properties are marked as changed because this is a full write 
            binaryWriter.Write(changedPropertiesMask);
            base.WriteFull(binaryWriter);
            MarkClean();
        }

        public override void MarkClean()
        {
            base.MarkClean();
            _changedProperties.Clear();
        }

        public override void Dispose()
        {
            Reference.PropertyChanged -= SourceObjectOnPropertyChanged;
        }
    }
}