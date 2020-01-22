using System;
using MonoSync.SyncSource;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class Int64FieldSerializer : FieldSerializer<long>
    {
        public override void Serialize(long value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public override void Deserialize(ExtendedBinaryReader reader, Action<long> valueFixup)
        {
            valueFixup(reader.ReadInt64());
        }

        public override bool CanInterpolate => true;

        public override long Interpolate(long source, long target, float factor)
        {
            return (long) (source + (target - source) * factor);
        }
    }
}