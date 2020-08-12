using MonoSync.Attributes;
using PropertyChanged;

namespace MonoSync.Test.TestObjects
{
    [Synchronizable]
    [AddINotifyPropertyChangedInterface]
    class NotifyPropertyChangedManualGetterOnlyMock
    {
        [Synchronize(SynchronizationBehaviour.Manual)]
        public int SomeValue { get; set; }

        public NotifyPropertyChangedManualGetterOnlyMock()
        {
            SomeValue = this.InitializeSynchronizableMember(nameof(SomeValue), () => 5);
        }
    }
}