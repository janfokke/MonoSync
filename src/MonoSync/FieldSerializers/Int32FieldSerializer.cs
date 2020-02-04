using System;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class Int32FieldSerializer : FieldSerializer<int>
    {
        public override bool CanInterpolate => true;

        public override void Serialize(int value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public override void Deserialize(ExtendedBinaryReader reader, Action<int> valueFixup)
        {
            valueFixup(reader.ReadInt32());
        }

        public override int Interpolate(int source, int target, float factor)
        {
            return (int) (source + (target - source) * factor);
        }
    }
}