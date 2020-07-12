using System;
using MonoSync.Utils;

namespace MonoSync.Serializers
{
    public class Int16Serializer : Serializer<short>
    {
        public override void Write(short value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public override void Read(ExtendedBinaryReader reader, Action<short> synchronizationCallback)
        {
            synchronizationCallback(reader.ReadInt16());
        }

        public override short Interpolate(short source, short target, float factor)
        {
            return (short) (source + (target - source) * factor);
        }
    }
}