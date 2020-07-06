using MonoSync.Exceptions;
using MonoSync.Test.TestObjects;
using MonoSync.Test.TestUtils;
using Xunit;

namespace MonoSync.Test.Synchronization
{
    public class NotifyPropertyChangedSyncSourceTests
    {
        public NotifyPropertyChangedSyncSourceTests()
        {
            _sourceSettings = SyncSourceSettings.Default;
            _targetSettings = SyncTargetSettings.Default;
        }

        private readonly SyncTargetSettings _targetSettings;
        private readonly SyncSourceSettings _sourceSettings;

        [Fact]
        public void Initializing_NonConstructorPropertyWithoutSetter_ThrowsSetterNotFoundException()
        {
            var attributeMarkedMethodMockSource = new GetterOnlyMock();

            var syncSourceRoot = new SyncSourceRoot(attributeMarkedMethodMockSource, _sourceSettings);

            Assert.Throws<SetterNotFoundException>(() =>
            {
                var syncTargetRoot = new SyncTargetRoot<GetterOnlyMock>(syncSourceRoot.WriteFullAndDispose(), _targetSettings);
            });
        }
    }
}