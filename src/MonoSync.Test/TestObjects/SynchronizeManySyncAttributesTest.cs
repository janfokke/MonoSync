using MonoSync.Attributes;
using PropertyChanged;

namespace MonoSync.Test.TestObjects
{
    [AddINotifyPropertyChangedInterface]
    internal class SynchronizeManySyncAttributesTest
    {
        [Sync]
        public int Test { get; set; }

        [Sync]
        public double Test2 { get; set; }

        [Sync]
        public float Test3 { get; set; }

        [Sync]
        public byte Test4 { get; set; }

        [Sync]
        public int Test5 { get; set; }

        [Sync]
        public double Test6 { get; set; }

        [Sync]
        public float Test7 { get; set; }

        [Sync]
        public byte Test8 { get; set; }

        [Sync]
        public double Test9 { get; set; }
    }
}