using System;
using Microsoft.Xna.Framework;
using MonoSync.Utils;

namespace MonoSync.Sample.Tweening
{
    public class Vector2Serializer : Serializer<Vector2>
    {
        public override void Write(Vector2 value, ExtendedBinaryWriter writer)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
        }

        public override void Read(ExtendedBinaryReader reader, Action<Vector2> synchronizationCallback)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            synchronizationCallback(new Vector2(x, y));
        }

        /// <remarks>
        /// When implementing <see cref="Interpolate"/>, make sure to also override <see cref="CanInterpolate"/> to return true.
        /// </remarks>
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