using System;
using MonoSync.SyncSource;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class Int16FieldSerializer : FieldSerializer<short>
    {
        public override void Serialize(short value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public override void Deserialize(ExtendedBinaryReader reader, Action<short> valueFixup)
        {
            valueFixup(reader.ReadInt16());
        }

        public override bool CanInterpolate => true;

        public override short Interpolate(short source, short target, float factor)
        {
            return (short) (source + (target - source) * factor);
        }
    }
}