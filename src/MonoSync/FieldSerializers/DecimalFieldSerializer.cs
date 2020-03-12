using System;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class DecimalFieldSerializer : FieldSerializer<decimal>
    {
        public override void Write(decimal value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public override void Read(ExtendedBinaryReader reader, Action<decimal> valueFixup)
        {
            valueFixup(reader.ReadDecimal());
        }

        public override decimal Interpolate(decimal source, decimal target, float factor)
        {
            return source + (target - source) * (decimal) factor;
        }
    }
}