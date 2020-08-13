using System;
using MonoSync.Attributes;
using PropertyChanged;

namespace MonoSync.Test.TestObjects
{
    [Synchronizable]
    [AddINotifyPropertyChangedInterface]
    public class ConstructedPropertyChangeSynchronizationMock
    {
        [Synchronize(SynchronizationBehaviour.Construction)]
        private readonly float _changeableProperty;

        public float Accessor => _changeableProperty;

        [SynchronizationConstructor]
        public ConstructedPropertyChangeSynchronizationMock(float changeableProperty)
        {
            _changeableProperty = changeableProperty;
        }
    }
}