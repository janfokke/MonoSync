using System;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class UInt64FieldSerializer : FieldSerializer<ulong>
    {
        public override void Write(ulong value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public override void Read(ExtendedBinaryReader reader, Action<ulong> valueFixup)
        {
            valueFixup(reader.ReadUInt64());
        }

        public override ulong Interpolate(ulong source, ulong target, float factor)
        {
            return (ulong) (source + (target - source) * factor);
        }
    }
}