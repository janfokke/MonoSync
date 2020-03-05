using System;
using System.Collections.Generic;
using System.Text;
using MonoSync.Exceptions;
using MonoSync.Test.TestObjects;
using MonoSync.Test.TestUtils;
using Xunit;

namespace MonoSync.Test.SyncObjectTests.NotifyPropertyChanged.SynchronizationBehaviour
{
    public class LatestTickTests
    {
        public LatestTickTests()
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
            typeEncoder.RegisterType<LatestTickMock>(index++);

            _sourceSettings = SyncSourceSettings.Default;
            _sourceSettings.TypeEncoder = typeEncoder;

            _targetSettings = SyncTargetSettings.Default;
            _targetSettings.TypeEncoder = typeEncoder;
        }

        private readonly SyncTargetSettings _targetSettings;
        private readonly SyncSourceSettings _sourceSettings;

        [Fact]
        public void Synchronizing_ValueWithLowerTickThanDirtyTick_ShouldNotBeSet()
        {
            var sourceObject = new LatestTickMock { Value = 5 };
            var syncSourceRoot = new SyncSourceRoot(sourceObject, _sourceSettings);
            
            var syncTargetRoot = new SyncTargetRoot<LatestTickMock>(syncSourceRoot.WriteFullAndDispose(), _targetSettings);
            var targetObject = syncTargetRoot.Root;
            Assert.Equal(5, targetObject.Value);

            syncTargetRoot.Clock.OwnTick = 10;
            targetObject.Value = 7;
            sourceObject.Value = 6;

            syncTargetRoot.Read(syncSourceRoot.WriteChangesAndDispose().SetTick(9));

            Assert.Equal(7,targetObject.Value);

            syncTargetRoot.Read(syncSourceRoot.WriteChangesAndDispose().SetTick(11));

            Assert.Equal(6, targetObject.Value);
        }
    }
}
