using System;
using Microsoft.Xna.Framework;

namespace MonoSync.Sample.Tweening
{
    public class Utils
    {
        public static Color RandomColor()
        {
            var rnd = new Random(Guid.NewGuid().GetHashCode());
            return new Color(rnd.Next(0, 256), rnd.Next(0, 256), rnd.Next(0, 256));
        }
    }
}