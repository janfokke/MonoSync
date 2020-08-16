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
            if (!TryGetMemberByName(e.PropertyName, out SynchronizableTargetMember syncTargetProperty))
            {
                return;
            }
            syncTargetProperty.NotifyChanged();
        }

        public override void Read(ExtendedBinaryReader reader)
        {
            BitArray changedProperties = reader.ReadBitArray(SynchronizableTargetMembers.Length);
            for (var index = 0; index < SynchronizableTargetMembers.Length; index++)
            {
                SynchronizableTargetMember synchronizableTargetMember = SynchronizableTargetMembers[index];
                if (changedProperties[index])
                {
                    synchronizableTargetMember.ReadChanges(reader);
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