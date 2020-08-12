using System;
using MonoSync.Utils;

namespace MonoSync.Serializers
{
    public class UInt16Serializer : Serializer<ushort>
    {
        public override void Write(ushort value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public override void Read(ExtendedBinaryReader reader, Action<ushort> synchronizationCallback)
        {
            synchronizationCallback(reader.ReadUInt16());
        }

        public override ushort Interpolate(ushort source, ushort target, float factor)
        {
            return (ushort) (source + (target - source) * factor);
        }
    }
}