using System;
using MonoSync.Exceptions;
using MonoSync.SyncSource;
using MonoSync.SyncTarget;
using MonoSync.SyncTarget.SyncTargetObjects;
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
            int index = StartingIndexNonReservedTypes;
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
        public void MoreThanEightSyncAttributesTest()
        {
            var sourceTestMock = new SynchronizeManySyncAttributesTest()
            {
                Test  = 0,
                Test2 = 2,
                Test3 = 34,
                Test4 = 43,
                Test5 = 122,
                Test6 = 99999999.32423,
                Test7 = 3434,
                Test8 = 23,
                Test9 = 2
            };

            var syncSourceRoot = new SyncSourceRoot(sourceTestMock, _sourceSettings);

            var target =
                new SyncTargetRoot<SynchronizeManySyncAttributesTest>(syncSourceRoot.WriteFullAndDispose(), _targetSettings);

            AssertExtension.AssertCloneEqual(sourceTestMock, target.Root);
        }

        [Fact]
        public void InitializingPropertyWithoutSetterShouldCauseSetterNotFoundException()
        {
            var attributeMarkedMethodMockSource = new GetterOnlyMock();

            var syncSourceRoot = new SyncSourceRoot(attributeMarkedMethodMockSource, _sourceSettings);

            Assert.Throws<SetterNotFoundException>(() =>
            {
                var syncTargetRoot = new SyncTargetRoot<GetterOnlyMock>(syncSourceRoot.WriteFullAndDispose(), _targetSettings);
            });
        }

        [Fact]
        public void InitializingPropertyWithoutSetterThroughConstructorTest()
        {
            var getterOnlyConstructorMockSource = new GetterOnlyConstructorMock(5);
            var syncSourceRoot = new SyncSourceRoot(getterOnlyConstructorMockSource, _sourceSettings);
            var syncTargetRoot = new SyncTargetRoot<GetterOnlyConstructorMock>(syncSourceRoot.WriteFullAndDispose(), _targetSettings);
            var getterOnlyConstructorMockRoot = syncTargetRoot.Root;
            Assert.Equal(getterOnlyConstructorMockSource.IntProperty, getterOnlyConstructorMockRoot.IntProperty);
        }

        [Fact]
        public void OnSynchronizedMarkedMethodShouldBeCalledAfterSynchronizationTest()
        {
            var attributeMarkedMethodMockSource = new OnSynchronizedAttributeMarkedMethodMock
            {
                intProperty = 123
            };

            var syncSourceRoot = new SyncSourceRoot(attributeMarkedMethodMockSource, _sourceSettings);
            var syncTargetRoot = new SyncTargetRoot<OnSynchronizedAttributeMarkedMethodMock>(syncSourceRoot.WriteFullAndDispose(), _targetSettings);

            OnSynchronizedAttributeMarkedMethodMock attributeMarkedMethodMockTarget = syncTargetRoot.Root;
            
            Assert.Equal(
                attributeMarkedMethodMockSource.intProperty, 
                attributeMarkedMethodMockTarget.intPropertyWhenSynchronizedMethodWasCalled
                );
        }

        [Fact]
        public void OnSynchronizedMarkedBaseMethodShouldBeCalledAfterSynchronizationTest()
        {
            var attributeMarkedMethodMockSource = new OnSynchronizedAttributeMarkedMethodMockChild
            {
                intProperty = 123
            };

            var syncSourceRoot = new SyncSourceRoot(attributeMarkedMethodMockSource, _sourceSettings);
            var syncTargetRoot = new SyncTargetRoot<OnSynchronizedAttributeMarkedMethodMockChild>(syncSourceRoot.WriteFullAndDispose(), _targetSettings);

            OnSynchronizedAttributeMarkedMethodMockChild attributeMarkedMethodMockTarget = syncTargetRoot.Root;

            Assert.Equal(
                attributeMarkedMethodMockSource.intProperty,
                attributeMarkedMethodMockTarget.intPropertyWhenSynchronizedMethodWasCalled
            );
        }

        [Fact]
        public void OnSynchronizedMarkedMethodShouldCauseSynchronizedMarkedMethodParameterException()
        {
            var attributeMarkedMethodMockSource = new OnSynchronizedAttributeMarkedMethodWithParametersMock();

            var syncSourceRoot = new SyncSourceRoot(attributeMarkedMethodMockSource, _sourceSettings);

            Assert.Throws<SynchronizedMarkedMethodParameterException>(() =>
            {
                var syncTargetRoot = new SyncTargetRoot<OnSynchronizedAttributeMarkedMethodWithParametersMock>(syncSourceRoot.WriteFullAndDispose(), _targetSettings);
            });
        }

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