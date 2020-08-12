using MonoSync.Attributes;
using PropertyChanged;

namespace MonoSync.Sample.Tweening
{
    [AddINotifyPropertyChangedInterface]
    public class Wall
    {
        [Sync(SynchronizationBehaviour.Construction)]
        public int X { get; set; }

        [Sync(SynchronizationBehaviour.Construction)]
        public int Y { get; set; }

        [Sync(SynchronizationBehaviour.Construction)]
        public int Height { get; set; }

        [Sync(SynchronizationBehaviour.Construction)]
        public int Width { get; set; }
    }
}