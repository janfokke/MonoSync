using System;

namespace MonoSync
{
    public interface ISerializerCollection
    {
        void AddSerializer(IFieldSerializer serializer);
        IFieldSerializer FindSerializerByType(Type type);
    }
}