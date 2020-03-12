using System;
using Microsoft.Xna.Framework;
using MonoSync.FieldSerializers;
using MonoSync.Utils;

namespace MonoSync.Sample.Tweening
{
    public class ColorSerializer : FieldSerializer<Color>
    {
        public override void Write(Color value, ExtendedBinaryWriter writer)
        {
            writer.Write(value.R);
            writer.Write(value.G);
            writer.Write(value.B);
            writer.Write(value.A);
        }

        public override void Read(ExtendedBinaryReader reader, Action<Color> valueFixup)
        {
            byte r = reader.ReadByte();
            byte g = reader.ReadByte();
            byte b = reader.ReadByte();
            byte a = reader.ReadByte();
            valueFixup(Color.FromNonPremultiplied(r, g, b, a));
        }
    }
}