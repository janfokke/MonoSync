using System;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class UInt32FieldSerializer : FieldSerializer<uint>
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