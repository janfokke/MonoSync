using System;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class UInt16FieldSerializer : FieldSerializer<ushort>
    {
        public override void Write(ushort value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public override void Read(ExtendedBinaryReader reader, Action<ushort> valueFixup)
        {
            valueFixup(reader.ReadUInt16());
        }

        public override ushort Interpolate(ushort source, ushort target, float factor)
        {
            return (ushort) (source + (target - source) * factor);
        }
    }
}