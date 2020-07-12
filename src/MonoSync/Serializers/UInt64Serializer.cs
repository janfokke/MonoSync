using System;
using MonoSync.Utils;

namespace MonoSync.Serializers
{
    public class UInt64Serializer : Serializer<ulong>
    {
        public override void Write(ulong value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public override void Read(ExtendedBinaryReader reader, Action<ulong> synchronizationCallback)
        {
            synchronizationCallback(reader.ReadUInt64());
        }

        public override ulong Interpolate(ulong source, ulong target, float factor)
        {
            return (ulong) (source + (target - source) * factor);
        }
    }
}