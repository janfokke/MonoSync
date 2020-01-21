using System;
using MonoSync.SyncSource;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class UInt16FieldSerializer : IFieldSerializer<ushort>
    {
        public void Serialize(ushort value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public void Deserialize(ExtendedBinaryReader reader, Action<ushort> valueFixup)
        {
            valueFixup(reader.ReadUInt16());
        }

        public bool CanInterpolate => true;

        public ushort Interpolate(ushort source, ushort target, float factor)
        {
            return (ushort) (source + (target - source) * factor);
        }
    }
}