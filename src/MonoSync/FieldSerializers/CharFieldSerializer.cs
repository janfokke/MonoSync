using System;
using MonoSync.SyncSource;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class CharFieldSerializer : IFieldSerializer<char>
    {
        public void Serialize(char value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public void Deserialize(ExtendedBinaryReader reader, Action<char> valueFixup)
        {
            valueFixup(reader.ReadChar());
        }
    }
}