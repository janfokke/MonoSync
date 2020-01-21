using System;
using MonoSync.SyncSource;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class Int16FieldSerializer : IFieldSerializer<short>
    {
        public void Serialize(short value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public void Deserialize(ExtendedBinaryReader reader, Action<short> valueFixup)
        {
            valueFixup(reader.ReadInt16());
        }

        public bool CanInterpolate => true;

        public short Interpolate(short source, short target, float factor)
        {
            return (short) (source + (target - source) * factor);
        }
    }
}