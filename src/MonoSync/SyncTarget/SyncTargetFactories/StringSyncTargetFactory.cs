using System;
using MonoSync.SyncTargetObjects;
using MonoSync.Utils;

namespace MonoSync.SyncTargetFactories
{
    public class StringSyncTargetFactory : ISyncTargetFactory
    {
        public bool CanCreate(Type baseType)
        {
            return typeof(string) == baseType;
        }

        public SynchronizerTarget Create(int referenceId, Type baseType, ExtendedBinaryReader reader,
            IFieldSerializerResolver fieldSerializerResolver, SyncTargetRoot root)
        {
            return new StringSynchronizerTarget(referenceId, reader);
        }
    }
}