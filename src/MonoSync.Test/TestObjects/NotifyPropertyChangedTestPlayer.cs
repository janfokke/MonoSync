using MonoSync.Attributes;
using PropertyChanged;

namespace MonoSync.Test.TestObjects
{
    [AddINotifyPropertyChangedInterface]
    internal class NotifyPropertyChangedTestPlayer
    {
        [Synchronize] public string Name { get; set; }

        [Synchronize] public int Level { get; set; }

        [Synchronize] public int Health { get; set; }
    }
}