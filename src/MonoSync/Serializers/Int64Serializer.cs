using System;
using MonoSync.Utils;

namespace MonoSync.Serializers
{
    public class Int64Serializer : Serializer<long>
    {
        public override void Write(long value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public override void Read(ExtendedBinaryReader reader, Action<long> synchronizationCallback)
        {
            synchronizationCallback(reader.ReadInt64());
        }

        public override long Interpolate(long source, long target, float factor)
        {
            return (long) (source + (target - source) * factor);
        }
    }
}