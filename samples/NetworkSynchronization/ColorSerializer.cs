using System;
using Microsoft.Xna.Framework;
using MonoSync.FieldSerializers;

namespace Tweening
{
    public class ColorSerializer : FieldSerializer<Color>
    {
        public override void Serialize(Color value, MonoSync.Utils.ExtendedBinaryWriter writer)
        {
            writer.Write(value.R);
            writer.Write(value.G);
            writer.Write(value.B);
            writer.Write(value.A);
        }

        public override void Deserialize(MonoSync.Utils.ExtendedBinaryReader reader, Action<Color> valueFixup)
        {
            byte r = reader.ReadByte();
            byte g = reader.ReadByte();
            byte b = reader.ReadByte();
            byte a = reader.ReadByte();
            valueFixup(Color.FromNonPremultiplied(r,g,b,a));
        }
    }
}