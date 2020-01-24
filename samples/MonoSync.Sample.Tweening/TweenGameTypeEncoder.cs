using Microsoft.Xna.Framework;

namespace MonoSync.Sample.Tweening
{
    public class TweenGameTypeEncoder : TypeEncoder
    {
        public TweenGameTypeEncoder()
        {
            int typeId = ReservedIdentifiers.StartingIndexNonReservedTypes;
            RegisterType<Map>(typeId++);
            RegisterType<Player>(typeId++);
            RegisterType<Vector2>(typeId++);
            RegisterType<Color>(typeId++);
        }
    }
}