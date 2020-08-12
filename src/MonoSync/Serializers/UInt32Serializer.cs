using System;
using MonoSync.Utils;

namespace MonoSync.Serializers
{
    public class UInt32Serializer : Serializer<uint>
    {
        public override void Write(uint value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public override void Read(ExtendedBinaryReader reader, Action<uint> synchronizationCallback)
        {
            synchronizationCallback(reader.ReadUInt32());
        }

        public override uint Interpolate(uint source, uint target, float factor)
        {
            return (uint) (source + (target - source) * factor);
        }
    }
}