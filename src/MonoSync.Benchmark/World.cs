using MonoSync.Attributes;
using MonoSync.Collections;
using PropertyChanged;

namespace MonoSync.Benchmark
{
    [AddINotifyPropertyChangedInterface]
    class World
    {
        [Sync]
        public ObservableDictionary<int, Entity> Entities { get; set; } = new ObservableDictionary<int, Entity>();
    }
}