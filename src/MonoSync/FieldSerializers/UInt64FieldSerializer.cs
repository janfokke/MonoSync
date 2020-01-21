using System;
using MonoSync.SyncSource;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class UInt64FieldSerializer : IFieldSerializer<ulong>
    {
        public void Serialize(ulong value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public void Deserialize(ExtendedBinaryReader reader, Action<ulong> valueFixup)
        {
            valueFixup(reader.ReadUInt64());
        }

        public bool CanInterpolate => true;

        public ulong Interpolate(ulong source, ulong target, float factor)
        {
            return (ulong) (source + (target - source) * factor);
        }
    }
}