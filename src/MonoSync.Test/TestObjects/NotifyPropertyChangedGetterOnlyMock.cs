using MonoSync.Attributes;
using PropertyChanged;

namespace MonoSync.Test.TestObjects
{
    [Synchronizable]
    [AddINotifyPropertyChangedInterface]
    public class NotifyPropertyChangedGetterOnlyMock
    {
        [Synchronize]
        public int IntProperty { get; }
    }
}