using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using FastMember;
using MonoSync.Utils;

namespace MonoSync.SyncSourceObjects
{
    public class NotifyPropertyChangedSyncSource : SyncSource
    {
        private readonly SortedDictionary<int, SyncSourceProperty> _changedProperties;
        private readonly PropertyCollection _propertyCollection;
        private readonly TypeAccessor _typeAccessor;

        public NotifyPropertyChangedSyncSource(SyncSourceRoot syncSourceRoot, int referenceId,
            INotifyPropertyChanged reference) :
            base(syncSourceRoot, referenceId, reference)
        {
            reference.PropertyChanged += SourceObjectOnPropertyChanged;
            Type baseType = reference.GetType();
            _typeAccessor = TypeAccessor.Create(baseType, false);
            _propertyCollection = syncSourceRoot.GetPropertiesFromType(baseType);
            _changedProperties = new SortedDictionary<int, SyncSourceProperty>();

            for (var i = 0; i < _propertyCollection.Length; i++)
            {
                SyncSourceProperty syncSourceProperty = _propertyCollection[i];
                if (!syncSourceProperty.IsValueType)
                {
                    object initialValue = _typeAccessor[Reference, syncSourceProperty.Name];
                    if (initialValue != null)
                    {
                        syncSourceRoot.TrackObject(initialValue);
                    }
                }
            }
        }

        public override void MarkClean()
        {
            base.MarkClean();
            _changedProperties.Clear();
        }

        private void SourceObjectOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var propertyName = e.PropertyName;

            // Property changed event might be from a non-sync property
            if (_propertyCollection.TryGetByName(e.PropertyName, out SyncSourceProperty syncSourceProperty))
            {
                object newValue = _typeAccessor[Reference, propertyName];

                if (syncSourceProperty.IsValueType == false)
                {
                    if (newValue != null)
                    {
                        SyncSourceRoot.TrackObject(newValue);
                    }
                }

                _changedProperties.TryAdd(syncSourceProperty.Index, syncSourceProperty);
                if (Dirty == false)
                {
                    MarkDirty();
                }
            }
        }

        public override void Dispose()
        {
            ((INotifyPropertyChanged) Reference).PropertyChanged -= SourceObjectOnPropertyChanged;
        }

        public override void WriteChanges(ExtendedBinaryWriter binaryWriter)
        {
            var changedPropertiesMask = new BitArray(_propertyCollection.Length);

            // Mark changed properties bitArray
            foreach (SyncSourceProperty syncSourceProperty in _changedProperties.Values)
            {
                changedPropertiesMask[syncSourceProperty.Index] = true;
            }

            binaryWriter.Write(changedPropertiesMask);

            foreach (SyncSourceProperty sourceProperty in _changedProperties.Values)
            {
                object value = _typeAccessor[Reference, sourceProperty.Name];
                sourceProperty.FieldSerializer.Write(value, binaryWriter);
            }

            MarkClean();
        }

        public override void WriteFull(ExtendedBinaryWriter binaryWriter)
        {
            var changedPropertiesMask = new BitArray(_propertyCollection.Length, true);

            // All properties are marked as changed because this is a full write 
            binaryWriter.Write(changedPropertiesMask);

            for (var index = 0; index < _propertyCollection.Length; index++)
            {
                SyncSourceProperty sourceProperty = _propertyCollection[index];
                object value = _typeAccessor[Reference, sourceProperty.Name];
                sourceProperty.FieldSerializer.Write(value, binaryWriter);
            }

            MarkClean();
        }
    }
}