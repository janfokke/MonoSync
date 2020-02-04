using System;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class ByteFieldSerializer : FieldSerializer<byte>
    {
        public override bool CanInterpolate => true;

        public override void Serialize(byte value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public override void Deserialize(ExtendedBinaryReader reader, Action<byte> valueFixup)
        {
            valueFixup(reader.ReadByte());
        }

        public override byte Interpolate(byte source, byte target, float factor)
        {
            return (byte) (source + (target - source) * factor);
        }
    }
}