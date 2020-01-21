using System;
using MonoSync.SyncSource;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class DoubleFieldSerializer : IFieldSerializer<double>
    {
        public void Serialize(double value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public void Deserialize(ExtendedBinaryReader reader, Action<double> valueFixup)
        {
            valueFixup(reader.ReadDouble());
        }

        public bool CanInterpolate => true;

        public double Interpolate(double source, double target, float factor)
        {
            return source + (target - source) * factor;
        }
    }
}