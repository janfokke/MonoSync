using MonoSync.Attributes;
using PropertyChanged;

namespace MonoSync.Test.TestObjects
{
    [AddINotifyPropertyChangedInterface]
    internal class TestPlayer
    {
        [Sync] public string Name { get; set; }

        [Sync] public int Level { get; set; }

        [Sync] public int Health { get; set; }
    }
}