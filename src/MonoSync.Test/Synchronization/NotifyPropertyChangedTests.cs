using MonoSync.SyncSource;
using MonoSync.SyncTarget;
using MonoSync.Test.TestObjects;
using MonoSync.Test.TestUtils;
using Xunit;
using static MonoSync.TypeEncoder.ReservedIdentifiers;

namespace MonoSync.Test.Synchronization
{
    public class NotifyPropertyChangedTests
    {
        public NotifyPropertyChangedTests()
        {
            var typeEncoder = new TypeEncoder();
            typeEncoder.RegisterType<SynchronizeConstructorMock>(StartingIndexNonReservedTypes);
            typeEncoder.RegisterType<TestPlayer>(StartingIndexNonReservedTypes + 1);

            _sourceSettings = SyncSourceSettings.Default;
            _sourceSettings.TypeEncoder = typeEncoder;

            _targetSettings = SyncTargetSettings.Default;
            _targetSettings.TypeEncoder = typeEncoder;
        }

        private readonly SyncTargetSettings _targetSettings;
        private readonly SyncSourceSettings _sourceSettings;

        [Fact]
        public void PropertiesUsedInConstructorShouldNotSynchronizeOnConstructionTest()
        {
            var sourceConstructorMock = new SynchronizeConstructorMock();

            var syncSourceRoot = new SyncSourceRoot(sourceConstructorMock, _sourceSettings);

            var syncTargetRoot =
                new SyncTargetRoot<SynchronizeConstructorMock>(syncSourceRoot.WriteFullAndDispose(), _targetSettings);
            SynchronizeConstructorMock targetConstructorMock = syncTargetRoot.Root;

            Assert.Equal(1, targetConstructorMock.DictionarySetCount);
        }

        [Fact]
        public void SyncConstructorShouldBeCalledTest()
        {
            var sourceConstructorMock = new SynchronizeConstructorMock();

            var syncSourceRoot = new SyncSourceRoot(sourceConstructorMock, _sourceSettings);

            var syncTargetRoot =
                new SyncTargetRoot<SynchronizeConstructorMock>(syncSourceRoot.WriteFullAndDispose(), _targetSettings);

            syncTargetRoot.Read(syncSourceRoot.WriteChangesAndDispose().SetTick(0));

            SynchronizeConstructorMock targetConstructorMock = syncTargetRoot.Root;

            Assert.True(targetConstructorMock.SyncConstructorCalled);
        }
    }
}