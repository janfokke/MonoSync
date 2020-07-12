using System;
using MonoSync.Utils;

namespace MonoSync.Serializers
{
    public class CharSerializer : Serializer<char>
    {
        public override void Write(char value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public override void Read(ExtendedBinaryReader reader, Action<char> synchronizationCallback)
        {
            synchronizationCallback(reader.ReadChar());
        }
    }
}