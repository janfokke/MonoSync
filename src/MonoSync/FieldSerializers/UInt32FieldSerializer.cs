using System;
using MonoSync.SyncSource;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class UInt32FieldSerializer : FieldSerializer<uint>
    {
        public override void Serialize(uint value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public override void Deserialize(ExtendedBinaryReader reader, Action<uint> valueFixup)
        {
            valueFixup(reader.ReadUInt32());
        }

        public override bool CanInterpolate => true;

        public override uint Interpolate(uint source, uint target, float factor)
        {
            return (uint) (source + (target - source) * factor);
        }
    }
}