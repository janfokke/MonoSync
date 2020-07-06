using System;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class SByteFieldSerializer : FieldSerializer<sbyte>
    {
        public override void Write(sbyte value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public override void Read(ExtendedBinaryReader reader, Action<sbyte> synchronizationCallback)
        {
            synchronizationCallback(reader.ReadSByte());
        }

        public override sbyte Interpolate(sbyte source, sbyte target, float factor)
        {
            return (sbyte) (source + (target - source) * factor);
        }
    }
}