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

        public override void Read(ExtendedBinaryReader reader, Action<Guid> valueFixup)
        {
            byte[] bytes = reader.ReadBytes(16);
            valueFixup(new Guid(bytes));
        }
    }
}