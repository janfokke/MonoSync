using System.Collections.Generic;
using MonoSync.Attributes;
using MonoSync.Collections;
using PropertyChanged;

namespace MonoSync.Benchmark
{
    [Synchronizable]
    [AddINotifyPropertyChangedInterface]
    class MonoSyncWorld
    {
        [Synchronize]
        public ObservableDictionary<int, Entity> Entities { get; set; } = new ObservableDictionary<int, Entity>();
    }

    public class JsonWorld
    {
        public Dictionary<int, Entity> Entities { get; set; } = new Dictionary<int, Entity>();
    }
}