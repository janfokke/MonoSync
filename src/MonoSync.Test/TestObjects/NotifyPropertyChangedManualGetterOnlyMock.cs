using MonoSync.Attributes;
using PropertyChanged;

namespace MonoSync.Test.TestObjects
{
    [AddINotifyPropertyChangedInterface]
    class NotifyPropertyChangedManualGetterOnlyMock
    {
        [Synchronize(SynchronizationBehaviour.Manual)]
        public int SomeValue { get; set; }

        public NotifyPropertyChangedManualGetterOnlyMock()
        {
            SomeValue = this.SynchronizeProperty(x => x.SomeValue, () => 5);
        }
    }
}