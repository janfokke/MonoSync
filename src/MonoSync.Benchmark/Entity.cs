using MonoSync.Attributes;
using PropertyChanged;

namespace MonoSync.Benchmark
{
    class Entity
    {
        [Synchronize]
        public int XPos { get; set; }
        [Synchronize]
        public int YPos { get; set; }
        [Synchronize]
        public int XVel { get; set; }
        [Synchronize]
        public int YVel { get; set; }
    }
}