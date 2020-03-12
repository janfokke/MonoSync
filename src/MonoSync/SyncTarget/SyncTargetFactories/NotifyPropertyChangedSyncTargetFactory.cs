using System;
using System.ComponentModel;
using MonoSync.SyncTargetObjects;
using MonoSync.Utils;

namespace MonoSync.SyncTargetFactories
{
    public class NotifyPropertyChangedSyncTargetFactory : ISyncTargetFactory
    {
        public bool CanCreate(Type baseType)
        {
            return typeof(INotifyPropertyChanged).IsAssignableFrom(baseType);
        }

        public SyncTarget Create(int referenceId, Type baseType, ExtendedBinaryReader reader,
            IFieldSerializerResolver fieldSerializerResolver, SyncTargetRoot clock)
        {
            return new NotifyPropertyChangedSyncTarget(referenceId, baseType, reader, clock, fieldSerializerResolver);
        }
    }
}