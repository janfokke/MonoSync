using MonoSync.Attributes;
using PropertyChanged;

namespace MonoSync.Test.TestObjects
{
    [AddINotifyPropertyChangedInterface]
    internal class LatestTickMock
    {
        [Sync(SynchronizationBehaviour.HighestTick)] public int Value { get; set; }
    }
}