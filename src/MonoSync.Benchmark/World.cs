using MonoSync.Attributes;
using MonoSync.Collections;
using PropertyChanged;

namespace MonoSync.Benchmark
{
    [AddINotifyPropertyChangedInterface]
    class World
    {
        [Synchronize]
        public ObservableDictionary<int, Entity> Entities { get; set; } = new ObservableDictionary<int, Entity>();
    }
}