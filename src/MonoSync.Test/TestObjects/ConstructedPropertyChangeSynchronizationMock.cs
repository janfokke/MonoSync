using MonoSync.Attributes;
using PropertyChanged;

namespace MonoSync.Test.TestObjects
{
    [AddINotifyPropertyChangedInterface]
    public class ConstructedPropertyChangeSynchronizationMock
    {
        [Sync]
        public float ChangeableProperty { get; set; }

        [SyncConstructor]
        public ConstructedPropertyChangeSynchronizationMock(float changeableProperty)
        {
            ChangeableProperty = changeableProperty;
        }
    }
}