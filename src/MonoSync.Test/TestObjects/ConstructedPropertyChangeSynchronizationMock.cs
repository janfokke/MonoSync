using System;
using MonoSync.Attributes;
using MonoSync.SyncTargetObjects;
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