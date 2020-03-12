using System;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class Int32FieldSerializer : FieldSerializer<int>
    {
        public override void Write(int value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public override void Read(ExtendedBinaryReader reader, Action<int> valueFixup)
        {
            valueFixup(reader.ReadInt32());
        }

        public override int Interpolate(int source, int target, float factor)
        {
            return (int) (source + (target - source) * factor);
        }
    }
}