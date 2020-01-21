using System;
using MonoSync.SyncSource;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class DecimalFieldSerializer : IFieldSerializer<decimal>
    {
        public void Serialize(decimal value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public void Deserialize(ExtendedBinaryReader reader, Action<decimal> valueFixup)
        {
            valueFixup(reader.ReadDecimal());
        }

        public bool CanInterpolate => true;

        public decimal Interpolate(decimal source, decimal target, float factor)
        {
            return source + (target - source) * (decimal) factor;
        }
    }
}