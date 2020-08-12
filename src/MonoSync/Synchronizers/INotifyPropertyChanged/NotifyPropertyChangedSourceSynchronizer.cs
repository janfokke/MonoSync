using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using MonoSync.Utils;

namespace MonoSync.Synchronizers
{
    public class NotifyPropertyChangedSourceSynchronizer : ObjectSourceSynchronizer
    {
        private new INotifyPropertyChanged Reference => (INotifyPropertyChanged)base.Reference;
        private readonly BitArray _changedProperties;
        public NotifyPropertyChangedSourceSynchronizer(SourceSynchronizerRoot sourceSynchronizerRoot, int referenceId, INotifyPropertyChanged reference) : base(sourceSynchronizerRoot, referenceId, reference)
        {
            _changedProperties = new BitArray(SourceMemberCollection.Length);
            reference.PropertyChanged += SourceObjectOnPropertyChanged;
        }

        private void SourceObjectOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Value changed event might be from a non-sync property
            if (SourceMemberCollection.TryGetPropertyByName(e.PropertyName, out SynchronizableSourceMember syncSourceProperty))
            {
                object newValue = syncSourceProperty.Value;

                // Synchronize newly added references
                if (syncSourceProperty.IsValueType == false)
                {
                    if (newValue != null)
                    {
                        SourceSynchronizerRoot.Synchronize(newValue);
                    }
                }
                _changedProperties[syncSourceProperty.Index] = true;
                if (Dirty == false)
                {
                    MarkDirty();
                }
            }
        }

        public override void WriteChanges(ExtendedBinaryWriter binaryWriter)
        {
            binaryWriter.Write(_changedProperties);
            for (var i = 0; i < _changedProperties.Count; i++)
            {
                if (_changedProperties[i])
                {
                    SourceMemberCollection[i].Serialize(binaryWriter);
                }
            }
            MarkClean();
        }

        public override void WriteFull(ExtendedBinaryWriter binaryWriter)
        {
            var changedPropertiesMask = new BitArray(SourceMemberCollection.Length, true);
            // All properties are marked as changed because this is a full write 
            binaryWriter.Write(changedPropertiesMask);
            base.WriteFull(binaryWriter);
            MarkClean();
        }

        public override void MarkClean()
        {
            base.MarkClean();
            _changedProperties.SetAll(false);
        }

        public override void Dispose()
        {
            Reference.PropertyChanged -= SourceObjectOnPropertyChanged;
        }
    }
}