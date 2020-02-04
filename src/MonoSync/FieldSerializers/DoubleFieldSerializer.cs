using System;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class DoubleFieldSerializer : FieldSerializer<double>
    {
        public override bool CanInterpolate => true;

        public override void Serialize(double value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public override void Deserialize(ExtendedBinaryReader reader, Action<double> valueFixup)
        {
            valueFixup(reader.ReadDouble());
        }

        public override double Interpolate(double source, double target, float factor)
        {
            return source + (target - source) * factor;
        }
    }
}