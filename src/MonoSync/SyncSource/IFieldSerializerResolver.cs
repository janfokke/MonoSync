using System;

namespace MonoSync.SyncSource
{
    public interface IFieldSerializerResolver
    {
        IFieldSerializer FindMatchingSerializer(Type type);
        void AddSerializer(IFieldSerializer serializer);
    }
}