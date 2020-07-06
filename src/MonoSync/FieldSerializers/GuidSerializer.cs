using System;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class GuidSerializer : FieldSerializer<Guid>
    {
        public override void Write(Guid value, ExtendedBinaryWriter writer)
        {
            writer.Write(value.ToByteArray());
        }

        public override void Read(ExtendedBinaryReader reader, Action<Guid> synchronizationCallback)
        {
            byte[] bytes = reader.ReadBytes(16);
            synchronizationCallback(new Guid(bytes));
        }
    }
}