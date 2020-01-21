using System;
using MonoSync.SyncSource;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class UInt32FieldSerializer : IFieldSerializer<uint>
    {
        public void Serialize(uint value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public void Deserialize(ExtendedBinaryReader reader, Action<uint> valueFixup)
        {
            valueFixup(reader.ReadUInt32());
        }

        public bool CanInterpolate => true;

        public uint Interpolate(uint source, uint target, float factor)
        {
            return (uint) (source + (target - source) * factor);
        }
    }
}