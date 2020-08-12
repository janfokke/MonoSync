using MonoSync.Attributes;
using MonoSync.Collections;
using PropertyChanged;

namespace MonoSync.Test.TestObjects
{
    [Synchronizable]
    [AddINotifyPropertyChangedInterface]
    internal class NotifyPropertyChangedTestGameWorld
    {
        [Synchronize]
        public ObservableDictionary<string, NotifyPropertyChangedTestPlayer> Players { get; set; } =
            new ObservableDictionary<string, NotifyPropertyChangedTestPlayer>();

        [Synchronize] public int RandomIntProperty { get; set; }
    }
}