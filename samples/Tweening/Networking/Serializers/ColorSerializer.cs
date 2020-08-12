using System;
using Microsoft.Xna.Framework;
using MonoSync.Utils;

namespace MonoSync.Sample.Tweening
{
    public class ColorSerializer : Serializer<Color>
    {
        public override void Write(Color value, ExtendedBinaryWriter writer)
        {
            writer.Write(value.R);
            writer.Write(value.G);
            writer.Write(value.B);
            writer.Write(value.A);
        }

        public override void Read(ExtendedBinaryReader reader, Action<Color> synchronizationCallback)
        {
            byte r = reader.ReadByte();
            byte g = reader.ReadByte();
            byte b = reader.ReadByte();
            byte a = reader.ReadByte();
            synchronizationCallback(Color.FromNonPremultiplied(r, g, b, a));
        }
    }
}