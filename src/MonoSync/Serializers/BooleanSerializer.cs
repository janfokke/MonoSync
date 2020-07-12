using System;
using MonoSync.Utils;

namespace MonoSync.Serializers
{
    public class BooleanSerializer : Serializer<bool>
    {
        public override void Write(bool value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public override void Read(ExtendedBinaryReader reader, Action<bool> synchronizationCallback)
        {
            synchronizationCallback(reader.ReadBoolean());
        }
    }
}