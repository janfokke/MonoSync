using System;
using MonoSync.Attributes;
using PropertyChanged;

namespace MonoSync.Test.TestObjects
{
    [Synchronizable]
    [AddINotifyPropertyChangedInterface]
    public class ConstructedPropertyChangeSynchronizationMock
    {
        [Synchronize]
        public float ChangeableProperty { get; set; }

        [SyncConstructor]
        public ConstructedPropertyChangeSynchronizationMock(float changeableProperty)
        {
            ChangeableProperty = changeableProperty;
        }
    }
}