using System;
using MonoSync.Exceptions;
using MonoSync.Test.TestObjects;
using MonoSync.Test.TestUtils;
using Xunit;

namespace MonoSync.Test.Synchronization
{
    public class NotifyPropertyChangedSyncTargetTests
    {
        [Fact]
        public void Initializing_NonConstructorPropertyWithoutSetter_ThrowsSetterNotFoundException()
        {
            var attributeMarkedMethodMockSource = new GetterOnlyMock();

            var SourceSynchronizerRoot = new SourceSynchronizerRoot(attributeMarkedMethodMockSource);

            Assert.Throws<SetterNotFoundException>(() =>
            {
                var TargetSynchronizerRoot = new TargetSynchronizerRoot(SourceSynchronizerRoot.WriteFullAndDispose());
            });
        }

        [Fact]
        public void Constructing_DependencyConstructorParameter_ResolvesFromDependencyResolver()
        {
            var dependencyConstructorMock = new ConstructedDependencyMock();

            var SourceSynchronizerRoot = new SourceSynchronizerRoot(dependencyConstructorMock);

            var TargetSynchronizerRoot = new TargetSynchronizerRoot<ConstructedDependencyMock>(SourceSynchronizerRoot.WriteFullAndDispose(), serviceProvider: new SomeServiceProvider());
            Assert.NotNull(TargetSynchronizerRoot.Reference.SomeService);
        }

        [Fact]
        public void Changing_ConstructorProperty_Synchronizes()
        {
            var getterOnlyConstructorMockSource = new ConstructedPropertyChangeSynchronizationMock(5f);
            
            var SourceSynchronizerRoot = new SourceSynchronizerRoot(getterOnlyConstructorMockSource);
            var TargetSynchronizerRoot = new TargetSynchronizerRoot<ConstructedPropertyChangeSynchronizationMock>(SourceSynchronizerRoot.WriteFullAndDispose());

            ConstructedPropertyChangeSynchronizationMock getterOnlyConstructorMockRoot = TargetSynchronizerRoot.Reference;
            Assert.Equal(getterOnlyConstructorMockSource.ChangeableProperty, getterOnlyConstructorMockRoot.ChangeableProperty);

            getterOnlyConstructorMockSource.ChangeableProperty = 6;
            TargetSynchronizerRoot.Read(SourceSynchronizerRoot.WriteChangesAndDispose().SetTick(0));

            Assert.Equal(getterOnlyConstructorMockSource.ChangeableProperty, getterOnlyConstructorMockRoot.ChangeableProperty);
        }

        [Fact]
        public void Synchronizing_PropertyWithoutSetterThroughConstructor_Synchronizes()
        {
            var getterOnlyConstructorMockSource = new GetterOnlyConstructorMock(5);
            var SourceSynchronizerRoot = new SourceSynchronizerRoot(getterOnlyConstructorMockSource);
            var TargetSynchronizerRoot = new TargetSynchronizerRoot<GetterOnlyConstructorMock>(SourceSynchronizerRoot.WriteFullAndDispose());
            GetterOnlyConstructorMock getterOnlyConstructorMockRoot = TargetSynchronizerRoot.Reference;
            Assert.Equal(getterOnlyConstructorMockSource.IntProperty, getterOnlyConstructorMockRoot.IntProperty);
        }

        [Fact]
        public void Synchronizing_WithOnSynchronizedCallback_InvokesCallback()
        {
            var attributeMarkedMethodMockSource = new OnSynchronizedAttributeMarkedMethodMock
            {
                IntProperty = 123
            };

            var SourceSynchronizerRoot = new SourceSynchronizerRoot(attributeMarkedMethodMockSource);
            var TargetSynchronizerRoot = new TargetSynchronizerRoot<OnSynchronizedAttributeMarkedMethodMock>(SourceSynchronizerRoot.WriteFullAndDispose());

            OnSynchronizedAttributeMarkedMethodMock attributeMarkedMethodMockTarget = TargetSynchronizerRoot.Reference;
            
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

            var SourceSynchronizerRoot = new SourceSynchronizerRoot(attributeMarkedMethodMockSource);
            var TargetSynchronizerRoot = new TargetSynchronizerRoot<OnSynchronizedAttributeMarkedMethodMockChild>(SourceSynchronizerRoot.WriteFullAndDispose());

            OnSynchronizedAttributeMarkedMethodMockChild attributeMarkedMethodMockTarget = TargetSynchronizerRoot.Reference;

            Assert.Equal(
                attributeMarkedMethodMockSource.IntProperty,
                attributeMarkedMethodMockTarget.IntPropertyWhenSynchronizedMethodWasCalled
            );
        }

        [Fact]
        public void Synchronizing_WithParmiterizedOnSynchronizedCallback_ThrowsParameterizedSynchronizedCallbackException()
        {
            var attributeMarkedMethodMockSource = new OnSynchronizedAttributeMarkedMethodWithParametersMock();

            var SourceSynchronizerRoot = new SourceSynchronizerRoot(attributeMarkedMethodMockSource);

            Assert.Throws<ParameterizedSynchronizedCallbackException>(() =>
            {
                var TargetSynchronizerRoot = new TargetSynchronizerRoot<OnSynchronizedAttributeMarkedMethodWithParametersMock>(SourceSynchronizerRoot.WriteFullAndDispose());
            });
        }

        [Fact]
        public void Synchronizing_PropertiesUsedAsConstructorParameters_ShouldNotSynchronizeAfterConstructor()
        {
            var sourceConstructorMock = new SynchronizeConstructorMock();

            var SourceSynchronizerRoot = new SourceSynchronizerRoot(sourceConstructorMock);

            var TargetSynchronizerRoot =
                new TargetSynchronizerRoot<SynchronizeConstructorMock>(SourceSynchronizerRoot.WriteFullAndDispose());
            SynchronizeConstructorMock targetConstructorMock = TargetSynchronizerRoot.Reference;

            Assert.Equal(1, targetConstructorMock.DictionarySetCount);
        }

        [Fact]
        public void Synchronizing_MarkedParameterizedConstructor_InvokesConstructorWithParameters()
        {
            var sourceConstructorMock = new SynchronizeConstructorMock();

            var SourceSynchronizerRoot = new SourceSynchronizerRoot(sourceConstructorMock);

            var TargetSynchronizerRoot =
                new TargetSynchronizerRoot<SynchronizeConstructorMock>(SourceSynchronizerRoot.WriteFullAndDispose());

            TargetSynchronizerRoot.Read(SourceSynchronizerRoot.WriteChangesAndDispose().SetTick(0));

            SynchronizeConstructorMock targetConstructorMock = TargetSynchronizerRoot.Reference;

            Assert.True(targetConstructorMock.SyncConstructorCalled);
        }
    }
}