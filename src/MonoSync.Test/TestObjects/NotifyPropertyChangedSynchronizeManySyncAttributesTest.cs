using MonoSync.Attributes;
using PropertyChanged;

namespace MonoSync.Test.TestObjects
{
    [Synchronizable]
    [AddINotifyPropertyChangedInterface]
    internal class NotifyPropertyChangedSynchronizeManySyncAttributesTest
    {
        [Synchronize]
        public int Test { get; set; }

        [Synchronize]
        public double Test2 { get; set; }

        [Synchronize]
        public float Test3 { get; set; }

        [Synchronize]
        public byte Test4 { get; set; }

        [Synchronize]
        public int Test5 { get; set; }

        [Synchronize]
        public double Test6 { get; set; }

        [Synchronize]
        public float Test7 { get; set; }

        [Synchronize]
        public byte Test8 { get; set; }

        [Synchronize]
        public double Test9 { get; set; }
    }
}