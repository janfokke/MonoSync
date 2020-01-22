using System;
using MonoSync.SyncSource;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class BooleanFieldSerializer : FieldSerializer<bool>
    {
        public override void Serialize(bool value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public override void Deserialize(ExtendedBinaryReader reader, Action<bool> valueFixup)
        {
            valueFixup(reader.ReadBoolean());
        }
    }
}