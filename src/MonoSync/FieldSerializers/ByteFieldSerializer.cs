using System;
using MonoSync.SyncSource;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class ByteFieldSerializer : IFieldSerializer<byte>
    {
        public void Serialize(byte value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public void Deserialize(ExtendedBinaryReader reader, Action<byte> valueFixup)
        {
            valueFixup(reader.ReadByte());
        }

        public bool CanInterpolate => true;

        public byte Interpolate(byte source, byte target, float factor)
        {
            return (byte) (source + (target - source) * factor);
        }
    }
}