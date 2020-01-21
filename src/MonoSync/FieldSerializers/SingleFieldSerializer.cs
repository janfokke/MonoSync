using System;
using MonoSync.SyncSource;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class SingleFieldSerializer : IFieldSerializer<float>
    {
        public void Serialize(float value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public void Deserialize(ExtendedBinaryReader reader, Action<float> valueFixup)
        {
            valueFixup(reader.ReadSingle());
        }

        public bool CanInterpolate => true;

        public float Interpolate(float source, float target, float factor)
        {
            return source + (target - source) * factor;
        }
    }
}