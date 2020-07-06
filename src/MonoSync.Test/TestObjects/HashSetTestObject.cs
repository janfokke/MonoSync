using MonoSync.Attributes;
using MonoSync.Collections;
using PropertyChanged;

namespace MonoSync.Test.TestObjects
{
    [AddINotifyPropertyChangedInterface]
    internal class HashSetTestObject
    {
        [Sync]
        public ObservableHashSet<TestPlayer> Players { get; set; } = new ObservableHashSet<TestPlayer>();

        [Sync] 
        public int RandomIntProperty { get; set; }
    }
}