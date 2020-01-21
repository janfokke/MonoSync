using System;
using MonoSync.SyncSource;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class SByteFieldSerializer : IFieldSerializer<sbyte>
    {
        public void Serialize(sbyte value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public void Deserialize(ExtendedBinaryReader reader, Action<sbyte> valueFixup)
        {
            valueFixup(reader.ReadSByte());
        }

        public bool CanInterpolate => true;

        public sbyte Interpolate(sbyte source, sbyte target, float factor)
        {
            return (sbyte) (source + (target - source) * factor);
        }
    }
}