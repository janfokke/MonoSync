using MonoSync.Attributes;
using PropertyChanged;

namespace MonoSync.Benchmark
{
    [AddINotifyPropertyChangedInterface]
    class Entity
    {
        [Sync]
        public int XPos { get; set; }
        [Sync]
        public int YPos { get; set; }
        [Sync]
        public int XVel { get; set; }
        [Sync]
        public int YVel { get; set; }
    }
}