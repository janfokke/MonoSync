using System;
using Microsoft.Xna.Framework;
using MonoSync.FieldSerializers;
using MonoSync.Utils;

namespace MonoSync.Sample.Tweening
{
    public class Vector2Serializer : FieldSerializer<Vector2>
    {
        public override bool CanInterpolate => true;

        public override void Serialize(Vector2 value, ExtendedBinaryWriter writer)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
        }

        public override void Deserialize(ExtendedBinaryReader reader, Action<Vector2> valueFixup)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            valueFixup(new Vector2(x, y));
        }

        public override Vector2 Interpolate(Vector2 source, Vector2 target, float factor)
        {
            var tmp = new Vector2
            {
                X = source.X + (target.X - source.X) * factor,
                Y = source.Y + (target.Y - source.Y) * factor
            };
            return tmp;
        }
    }
}