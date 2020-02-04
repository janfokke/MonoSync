using System;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class DecimalFieldSerializer : FieldSerializer<decimal>
    {
        public override bool CanInterpolate => true;

        public override void Serialize(decimal value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public override void Deserialize(ExtendedBinaryReader reader, Action<decimal> valueFixup)
        {
            valueFixup(reader.ReadDecimal());
        }

        public override decimal Interpolate(decimal source, decimal target, float factor)
        {
            return source + (target - source) * (decimal) factor;
        }
    }
}