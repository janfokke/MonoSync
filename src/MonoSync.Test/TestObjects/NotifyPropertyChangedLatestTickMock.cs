using MonoSync.Attributes;
using PropertyChanged;

namespace MonoSync.Test.TestObjects
{
    [Synchronizable]
    [AddINotifyPropertyChangedInterface]
    internal class NotifyPropertyChangedLatestTickMock
    {
        [Synchronize(SynchronizationBehaviour.HighestTick)] public int Value { get; set; }
    }
}