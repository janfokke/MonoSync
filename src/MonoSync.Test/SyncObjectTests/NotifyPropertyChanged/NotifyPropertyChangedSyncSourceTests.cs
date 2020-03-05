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
            var typeEncoder = new TypeEncoder();
            int index = TypeEncoder.ReservedIdentifiers.StartingIndexNonReservedTypes;
            typeEncoder.RegisterType<SynchronizeConstructorMock>(index++);
            typeEncoder.RegisterType<TestPlayer>(index++);
            typeEncoder.RegisterType<SynchronizeManySyncAttributesTest>(index++);
            typeEncoder.RegisterType<OnSynchronizedAttributeMarkedMethodMock>(index++);
            typeEncoder.RegisterType<OnSynchronizedAttributeMarkedMethodMockChild>(index++);
            typeEncoder.RegisterType<OnSynchronizedAttributeMarkedMethodWithParametersMock>(index++);
            typeEncoder.RegisterType<GetterOnlyMock>(index++);
            typeEncoder.RegisterType<GetterOnlyConstructorMock>(index++);

            _sourceSettings = SyncSourceSettings.Default;
            _sourceSettings.TypeEncoder = typeEncoder;

            _targetSettings = SyncTargetSettings.Default;
            _targetSettings.TypeEncoder = typeEncoder;
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