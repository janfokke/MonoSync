using System;
using System.Collections;
using System.ComponentModel;
using MonoSync.Utils;

namespace MonoSync.Synchronizers
{
    public class NotifyPropertyChangedTargetSynchronizer : ObjectTargetSynchronizer
    {
        private new INotifyPropertyChanged Reference
        {
            get => (INotifyPropertyChanged)base.Reference;
            set => base.Reference = value;
        }

        public NotifyPropertyChangedTargetSynchronizer(TargetSynchronizerRoot targetSynchronizerRoot, int referenceId, Type referenceType) : 
            base(targetSynchronizerRoot, referenceId, referenceType)
        {
            Reference.PropertyChanged += TargetOnPropertyChanged;
        }

        private void TargetOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!TargetPropertyByNameLookup.TryGetValue(e.PropertyName, out SyncTargetProperty syncTargetProperty))
            {
                return;
            }
            syncTargetProperty.NotifyChanged();
        }

        public override void Read(ExtendedBinaryReader reader)
        {
            BitArray changedProperties = reader.ReadBitArray(TargetPropertyByIndexLookup.Length);
            for (var index = 0; index < TargetPropertyByIndexLookup.Length; index++)
            {
                SyncTargetProperty syncTargetProperty = TargetPropertyByIndexLookup[index];
                if (changedProperties[index])
                {
                    syncTargetProperty.ReadChanges(reader);
                }
            }
        }

        public override void Dispose()
        {
            Reference.PropertyChanged -= TargetOnPropertyChanged;
            base.Dispose();
        }
    }
}