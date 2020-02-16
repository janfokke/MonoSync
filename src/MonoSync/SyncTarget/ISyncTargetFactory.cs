using System;
using MonoSync.Utils;

namespace MonoSync
{
    public interface ISyncTargetFactory
    {
        bool CanCreate(Type baseType);

        SyncTarget Create(int referenceId, Type baseType, ExtendedBinaryReader reader,
            IFieldSerializerResolver fieldSerializerResolver, SyncTargetRoot root);
    }
}