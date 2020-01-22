using System;
using MonoSync.SyncSource;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class CharFieldSerializer : FieldSerializer<char>
    {
        public override void Serialize(char value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public override void Deserialize(ExtendedBinaryReader reader, Action<char> valueFixup)
        {
            valueFixup(reader.ReadChar());
        }
    }
}