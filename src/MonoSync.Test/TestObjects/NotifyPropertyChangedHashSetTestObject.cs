using MonoSync.Attributes;
using MonoSync.Collections;
using PropertyChanged;

namespace MonoSync.Test.TestObjects
{
    [Synchronizable]
    [AddINotifyPropertyChangedInterface]
    internal class NotifyPropertyChangedHashSetTestObject
    {
        [Synchronize]
        public ObservableHashSet<NotifyPropertyChangedTestPlayer> Players { get; set; } = new ObservableHashSet<NotifyPropertyChangedTestPlayer>();

        [Synchronize] 
        public int RandomIntProperty { get; set; }
    }
}