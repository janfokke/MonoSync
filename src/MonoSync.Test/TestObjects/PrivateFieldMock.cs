using MonoSync.Attributes;

namespace MonoSync.Test.Synchronization
{
    [Synchronizable]
    public class PrivateFieldMock
    {
        [Synchronize(SynchronizationBehaviour.Manual)]
        private readonly int _test;

        // ReSharper disable once ConvertToAutoProperty
        public int GetTestValue => _test;

        [SynchronizationConstructor]
        public PrivateFieldMock()
        {
            _test = this.InitializeSynchronizableMember(nameof(_test), () => 0);
        }

        public PrivateFieldMock(int value)
        {
            _test = value;
        }
    }
}