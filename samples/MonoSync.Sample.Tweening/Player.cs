using Microsoft.Xna.Framework;
using MonoSync.Attributes;
using PropertyChanged;

namespace MonoSync.Sample.Tweening
{
    [AddINotifyPropertyChangedInterface]
    public class Player
    {
        public Player()
        {
        }

        public Player(Color color)
        {
            Color = color;
        }

        [Sync(SynchronizationBehaviour.Interpolated)]
        public Vector2 Position { get; set; }

        [Sync] public Vector2 TargetPosition { get; set; }

        [Sync] public Color Color { get; set; }
    }
}