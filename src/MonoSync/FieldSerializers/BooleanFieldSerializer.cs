using System;
using MonoSync.SyncSource;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class BooleanFieldSerializer : IFieldSerializer<bool>
    {
        public void Serialize(bool value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public void Deserialize(ExtendedBinaryReader reader, Action<bool> valueFixup)
        {
            valueFixup(reader.ReadBoolean());
        }
    }
}