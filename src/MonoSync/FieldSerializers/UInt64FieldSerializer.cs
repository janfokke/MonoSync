using System;
using MonoSync.SyncSource;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class UInt64FieldSerializer : FieldSerializer<ulong>
    {
        public override void Serialize(ulong value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public override void Deserialize(ExtendedBinaryReader reader, Action<ulong> valueFixup)
        {
            valueFixup(reader.ReadUInt64());
        }

        public override bool CanInterpolate => true;

        public override ulong Interpolate(ulong source, ulong target, float factor)
        {
            return (ulong) (source + (target - source) * factor);
        }
    }
}