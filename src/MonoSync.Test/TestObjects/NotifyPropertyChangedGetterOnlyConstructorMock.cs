using MonoSync.Attributes;
using PropertyChanged;

namespace MonoSync.Test.TestObjects
{
    [Synchronizable]
    [AddINotifyPropertyChangedInterface]
    public class NotifyPropertyChangedGetterOnlyConstructorMock
    {
        [SyncConstructor]
        public NotifyPropertyChangedGetterOnlyConstructorMock(int intProperty)
        {
            IntProperty = intProperty;
        }

        [Synchronize(SynchronizationBehaviour.Construction)]
        public int IntProperty { get; }
    }
}