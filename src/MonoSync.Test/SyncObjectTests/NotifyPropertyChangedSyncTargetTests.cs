using System;
using MonoSync.Exceptions;
using MonoSync.Test.TestObjects;
using MonoSync.Test.TestUtils;
using Xunit;
using static MonoSync.TypeEncoder.ReservedIdentifiers;

namespace MonoSync.Test.Synchronization
{
    public class NotifyPropertyChangedSyncTargetTests
    {
        public NotifyPropertyChangedSyncTargetTests()
        {
            var typeEncoder = new TypeEncoder();
            int index = StartingIndexNonReservedTypes;
            typeEncoder.RegisterType<SynchronizeConstructorMock>(index++);
            typeEncoder.RegisterType<TestPlayer>(index++);
            typeEncoder.RegisterType<SynchronizeManySyncAttributesTest>(index++);
            typeEncoder.RegisterType<OnSynchronizedAttributeMarkedMethodMock>(index++);
            typeEncoder.RegisterType<OnSynchronizedAttributeMarkedMethodMockChild>(index++);
            typeEncoder.RegisterType<OnSynchronizedAttributeMarkedMethodWithParametersMock>(index++);
            typeEncoder.RegisterType<ConstructedPropertyChangeSynchronizationMock>(index++);
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

        [Fact]
        public void Changing_ConstructorProperty_Synchronizes()
        {
            var getterOnlyConstructorMockSource = new ConstructedPropertyChangeSynchronizationMock(5f);
            
            var syncSourceRoot = new SyncSourceRoot(getterOnlyConstructorMockSource, _sourceSettings);
            var syncTargetRoot = new SyncTargetRoot<ConstructedPropertyChangeSynchronizationMock>(syncSourceRoot.WriteFullAndDispose(), _targetSettings);

            ConstructedPropertyChangeSynchronizationMock getterOnlyConstructorMockRoot = syncTargetRoot.Root;
            Assert.Equal(getterOnlyConstructorMockSource.ChangeableProperty, getterOnlyConstructorMockRoot.ChangeableProperty);

            getterOnlyConstructorMockSource.ChangeableProperty = 6;
            syncTargetRoot.Read(syncSourceRoot.WriteChangesAndDispose().SetTick(0));

            Assert.Equal(getterOnlyConstructorMockSource.ChangeableProperty, getterOnlyConstructorMockRoot.ChangeableProperty);
        }

        [Fact]
        public void Synchronizing_PropertyWithoutSetterThroughConstructor_Synchronizes()
        {
            var getterOnlyConstructorMockSource = new GetterOnlyConstructorMock(5);
            var syncSourceRoot = new SyncSourceRoot(getterOnlyConstructorMockSource, _sourceSettings);
            var syncTargetRoot = new SyncTargetRoot<GetterOnlyConstructorMock>(syncSourceRoot.WriteFullAndDispose(), _targetSettings);
            GetterOnlyConstructorMock getterOnlyConstructorMockRoot = syncTargetRoot.Root;
            Assert.Equal(getterOnlyConstructorMockSource.IntProperty, getterOnlyConstructorMockRoot.IntProperty);
        }

        [Fact]
        public void Synchronizing_WithOnSynchronizedCallback_InvokesCallback()
        {
            var attributeMarkedMethodMockSource = new OnSynchronizedAttributeMarkedMethodMock
            {
                IntProperty = 123
            };

            var syncSourceRoot = new SyncSourceRoot(attributeMarkedMethodMockSource, _sourceSettings);
            var syncTargetRoot = new SyncTargetRoot<OnSynchronizedAttributeMarkedMethodMock>(syncSourceRoot.WriteFullAndDispose(), _targetSettings);

            OnSynchronizedAttributeMarkedMethodMock attributeMarkedMethodMockTarget = syncTargetRoot.Root;
            
            Assert.Equal(
                attributeMarkedMethodMockSource.IntProperty, 
                attributeMarkedMethodMockTarget.IntPropertyWhenSynchronizedMethodWasCalled
                );
        }

        [Fact]
        public void Synchronizing_WithOnSynchronizedCallbackInBaseClass_InvokesCallback()
        {
            var attributeMarkedMethodMockSource = new OnSynchronizedAttributeMarkedMethodMockChild
            {
                IntProperty = 123
            };

            var syncSourceRoot = new SyncSourceRoot(attributeMarkedMethodMockSource, _sourceSettings);
            var syncTargetRoot = new SyncTargetRoot<OnSynchronizedAttributeMarkedMethodMockChild>(syncSourceRoot.WriteFullAndDispose(), _targetSettings);

            OnSynchronizedAttributeMarkedMethodMockChild attributeMarkedMethodMockTarget = syncTargetRoot.Root;

            Assert.Equal(
                attributeMarkedMethodMockSource.IntProperty,
                attributeMarkedMethodMockTarget.IntPropertyWhenSynchronizedMethodWasCalled
            );
        }

        [Fact]
        public void Synchronizing_WithParmiterizedOnSynchronizedCallback_ThrowsParameterizedSynchronizedCallbackException()
        {
            var attributeMarkedMethodMockSource = new OnSynchronizedAttributeMarkedMethodWithParametersMock();

            var syncSourceRoot = new SyncSourceRoot(attributeMarkedMethodMockSource, _sourceSettings);

            Assert.Throws<ParameterizedSynchronizedCallbackException>(() =>
            {
                var syncTargetRoot = new SyncTargetRoot<OnSynchronizedAttributeMarkedMethodWithParametersMock>(syncSourceRoot.WriteFullAndDispose(), _targetSettings);
            });
        }

        [Fact]
        public void Synchronizing_PropertiesUsedAsConstructorParameters_ShouldNotSynchronizeAfterConstructor()
        {
            var sourceConstructorMock = new SynchronizeConstructorMock();

            var syncSourceRoot = new SyncSourceRoot(sourceConstructorMock, _sourceSettings);

            var syncTargetRoot =
                new SyncTargetRoot<SynchronizeConstructorMock>(syncSourceRoot.WriteFullAndDispose(), _targetSettings);
            SynchronizeConstructorMock targetConstructorMock = syncTargetRoot.Root;

            Assert.Equal(1, targetConstructorMock.DictionarySetCount);
        }

        [Fact]
        public void Synchronizing_MarkedParameterizedConstructor_InvokesConstructorWithParameters()
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