using System;
using MonoSync.SyncSource;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class Int64FieldSerializer : IFieldSerializer<long>
    {
        public void Serialize(long value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public void Deserialize(ExtendedBinaryReader reader, Action<long> valueFixup)
        {
            valueFixup(reader.ReadInt64());
        }

        public bool CanInterpolate => true;

        public long Interpolate(long source, long target, float factor)
        {
            return (long) (source + (target - source) * factor);
        }
    }
}