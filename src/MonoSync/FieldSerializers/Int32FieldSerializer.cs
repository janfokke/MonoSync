using System;
using MonoSync.SyncSource;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class Int32FieldSerializer : IFieldSerializer<int>
    {
        public void Serialize(int value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public void Deserialize(ExtendedBinaryReader reader, Action<int> valueFixup)
        {
            valueFixup(reader.ReadInt32());
        }

        public bool CanInterpolate => true;

        public int Interpolate(int source, int target, float factor)
        {
            return (int) (source + (target - source) * factor);
        }
    }
}