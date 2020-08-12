using MonoSync.Attributes;
using MonoSync.Collections;
using PropertyChanged;

namespace MonoSync.Benchmark
{
    [Synchronizable]
    [AddINotifyPropertyChangedInterface]
    class World
    {
        [Synchronize]
        public ObservableDictionary<int, Entity> Entities { get; set; } = new ObservableDictionary<int, Entity>();
    }
}