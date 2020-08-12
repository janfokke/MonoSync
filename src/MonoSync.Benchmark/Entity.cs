using MonoSync.Attributes;
using PropertyChanged;

namespace MonoSync.Benchmark
{
    [Synchronizable]
    [AddINotifyPropertyChangedInterface]
    class Entity
    {
        public Entity()
        {
            XPos = this.InitializeSynchronizableMember(nameof(XPos), () => default(int));
            YPos = this.InitializeSynchronizableMember(nameof(YPos), () => default(int));
            XVel = this.InitializeSynchronizableMember(nameof(XVel), () => default(int));
            YVel = this.InitializeSynchronizableMember(nameof(YVel), () => default(int));
        }

        [Synchronize(SynchronizationBehaviour.Manual)]
        public int XPos { get; set; }
        [Synchronize(SynchronizationBehaviour.Manual)]
        public int YPos { get; set; }
        [Synchronize(SynchronizationBehaviour.Manual)]
        public int XVel { get; set; }
        [Synchronize(SynchronizationBehaviour.Manual)]
        public int YVel { get; set; }
    }
}