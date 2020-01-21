using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using MonoSync.Attributes;
using MonoSync.Utils;

namespace MonoSync.SyncSource.SyncSourceObjects
{
    public class NotifyPropertyChangedSyncSource : SyncSource
    {
        private readonly SourcePropertyChangeTracker<SyncSourceProperty> _changeTracker =
            new SourcePropertyChangeTracker<SyncSourceProperty>();

        private readonly Dictionary<string, SyncSourceProperty> _propertyLookup =
            new Dictionary<string, SyncSourceProperty>();

        private readonly SyncSourceProperty[] _syncSourceProperties;

        protected readonly List<SyncSourceProperty> ReferenceProperties = new List<SyncSourceProperty>();

        public NotifyPropertyChangedSyncSource(SyncSourceRoot syncSourceRoot, int referenceId,
            INotifyPropertyChanged baseObject,
            IFieldSerializerResolver fieldSerializerResolver) :
            base(syncSourceRoot, referenceId, baseObject)
        {
            baseObject.PropertyChanged += SourceObjectOnPropertyChanged;

            _changeTracker.Dirty += ChangeTrackerOnDirty;
            PropertyInfo[] properties = BaseObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            (PropertyInfo info, SyncAttribute attibute)[] syncProperties =
                PropertyInfoHelper.GetSyncProperties(properties).ToArray();
            _syncSourceProperties = new SyncSourceProperty[syncProperties.Length];
            for (var syncPropertyIndex = 0; syncPropertyIndex < syncProperties.Length; syncPropertyIndex++)
            {
                PropertyInfo syncPropertyInfo = syncProperties[syncPropertyIndex].info;

                Type propertyType = syncPropertyInfo.PropertyType;

                Func<object> getter = PropertyInfoHelper.CreateGetterDelegate(syncPropertyInfo, BaseObject);

                var property = new SyncSourceProperty(syncPropertyIndex, propertyType, getter,
                    fieldSerializerResolver.FindMatchingSerializer(propertyType));

                _propertyLookup[syncPropertyInfo.Name] = property;
                _syncSourceProperties[syncPropertyIndex] = property;

                property.UpdateValue();

                // Reference properties getters are added to a list for garbage collection.
                if (!propertyType.IsValueType)
                {
                    object propertyValue = property.Value;
                    if (propertyValue != null)
                    {
                        syncSourceRoot.AddReference(propertyValue);
                    }

                    ReferenceProperties.Add(property);
                }
            }
        }

        private void ChangeTrackerOnDirty(object sender, EventArgs e)
        {
            base.MarkDirty();
        }

        private void SourceObjectOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_propertyLookup.TryGetValue(e.PropertyName, out SyncSourceProperty syncSourceProperty))
            {
                syncSourceProperty.UpdateValue();

                object previousValue = syncSourceProperty.PreviousValue;
                object newValue = syncSourceProperty.Value;

                if (syncSourceProperty.PropertyType.IsValueType == false)
                {
                    if (previousValue != null)
                    {
                        SyncSourceRoot.RemoveReference(previousValue);
                    }

                    if (newValue != null)
                    {
                        SyncSourceRoot.AddReference(newValue);
                    }
                }

                _changeTracker.MarkDirty(syncSourceProperty);
            }
        }

        public override IEnumerable<object> GetReferences()
        {
            return ReferenceProperties.Select(x => x.Value);
        }

        public override void Dispose()
        {
            ((INotifyPropertyChanged) BaseObject).PropertyChanged -= SourceObjectOnPropertyChanged;
        }

        public override void WriteChanges(ExtendedBinaryWriter binaryWriter)
        {
            var changedPropertiesBitArray = new BitArray(_syncSourceProperties.Length);

            List<SyncSourceProperty> syncSourceProperties = _changeTracker.ToList();

            // Mark changed properties bitArray
            foreach (SyncSourceProperty syncSourceProperty in syncSourceProperties)
            {
                changedPropertiesBitArray[syncSourceProperty.Index] = true;
            }

            binaryWriter.Write(changedPropertiesBitArray);

            foreach (SyncSourceProperty sourceProperty in syncSourceProperties)
            {
                sourceProperty.Serialize(binaryWriter);
            }

            _changeTracker.Clear();
        }

        public override void WriteFull(ExtendedBinaryWriter binaryWriter)
        {
            var changedProperties = new BitArray(_syncSourceProperties.Length, true);

            // All properties are marked as changed because this is a full write 
            binaryWriter.Write(changedProperties);

            for (var index = 0; index < _syncSourceProperties.Length; index++)
            {
                SyncSourceProperty syncSourceProperty = _syncSourceProperties[index];
                syncSourceProperty.Serialize(binaryWriter);
            }

            _changeTracker.Clear();
        }
    }
}