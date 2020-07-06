using System;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class BooleanFieldSerializer : FieldSerializer<bool>
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