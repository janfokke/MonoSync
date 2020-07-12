using System;
using MonoSync.Utils;

namespace MonoSync.Serializers
{
    public class GuidSerializer : Serializer<Guid>
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