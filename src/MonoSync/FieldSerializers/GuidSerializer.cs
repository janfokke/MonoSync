using System;
using MonoSync.SyncSource;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class GuidSerializer : IFieldSerializer<Guid>
    {
        public void Serialize(Guid value, ExtendedBinaryWriter writer)
        {
            writer.Write(value.ToByteArray());
        }

        public void Deserialize(ExtendedBinaryReader reader, Action<Guid> valueFixup)
        {
            byte[] bytes = reader.ReadBytes(16);
            valueFixup(new Guid(bytes));
        }
    }
}