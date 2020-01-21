using System;
using MonoSync.SyncSource;
using MonoSync.Utils;

namespace MonoSync.SyncTarget
{
    public interface ISyncTargetFactory
    {
        bool CanCreate(Type baseType);

        SyncTarget Create(int referenceId, Type baseType, ExtendedBinaryReader reader,
            IFieldSerializerResolver fieldSerializerResolver, SyncTargetRoot root);
    }
}