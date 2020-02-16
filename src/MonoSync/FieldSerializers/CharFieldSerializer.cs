using System;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class CharFieldSerializer : FieldSerializer<char>
    {
        public override void Write(char value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public override void Read(ExtendedBinaryReader reader, Action<char> valueFixup)
        {
            valueFixup(reader.ReadChar());
        }
    }
}