using MonoSync.Attributes;
using MonoSync.Collections;
using PropertyChanged;

namespace MonoSync.Test.TestObjects
{
    [AddINotifyPropertyChangedInterface]
    internal class TestGameWorld
    {
        [Sync]
        public ObservableDictionary<string, TestPlayer> Players { get; set; } =
            new ObservableDictionary<string, TestPlayer>();

        [Sync] public int RandomIntProperty { get; set; }
    }
}