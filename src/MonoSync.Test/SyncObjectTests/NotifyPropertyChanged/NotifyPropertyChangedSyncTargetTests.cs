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
            var attributeMarkedMethodMockSource = new NotifyPropertyChangedGetterOnlyMock();

            var SourceSynchronizerRoot = new SourceSynchronizerRoot(attributeMarkedMethodMockSource);

            Assert.Throws<SetterNotFoundException>(() =>
            {
                var TargetSynchronizerRoot = new TargetSynchronizerRoot(SourceSynchronizerRoot.WriteFullAndDispose());
            });
        }

        [Fact]
        public void Constructing_DependencyConstructorParameter_ResolvesFromDependencyResolver()
        {
            var dependencyConstructorMock = new NotifyPropertyChangedConstructedDependencyMock();

            var SourceSynchronizerRoot = new SourceSynchronizerRoot(dependencyConstructorMock);

            var TargetSynchronizerRoot = new TargetSynchronizerRoot<NotifyPropertyChangedConstructedDependencyMock>(SourceSynchronizerRoot.WriteFullAndDispose(), serviceProvider: new TestServiceProvider());
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
            var getterOnlyConstructorMockSource = new NotifyPropertyChangedGetterOnlyConstructorMock(5);
            var SourceSynchronizerRoot = new SourceSynchronizerRoot(getterOnlyConstructorMockSource);
            var TargetSynchronizerRoot = new TargetSynchronizerRoot<NotifyPropertyChangedGetterOnlyConstructorMock>(SourceSynchronizerRoot.WriteFullAndDispose());
            NotifyPropertyChangedGetterOnlyConstructorMock notifyPropertyChangedGetterOnlyConstructorMockRoot = TargetSynchronizerRoot.Reference;
            Assert.Equal(getterOnlyConstructorMockSource.IntProperty, notifyPropertyChangedGetterOnlyConstructorMockRoot.IntProperty);
        }

        [Fact]
        public void Synchronizing_PropertiesUsedAsConstructorParameters_ShouldNotSynchronizeAfterConstructor()
        {
            var sourceConstructorMock = new NotifyPropertyChangedSynchronizeConstructorMock();

            var SourceSynchronizerRoot = new SourceSynchronizerRoot(sourceConstructorMock);

            var TargetSynchronizerRoot =
                new TargetSynchronizerRoot<NotifyPropertyChangedSynchronizeConstructorMock>(SourceSynchronizerRoot.WriteFullAndDispose());
            NotifyPropertyChangedSynchronizeConstructorMock targetConstructorMock = TargetSynchronizerRoot.Reference;

            Assert.Equal(1, targetConstructorMock.DictionarySetCount);
        }

        [Fact]
        public void Synchronizing_MarkedParameterizedConstructor_InvokesConstructorWithParameters()
        {
            var sourceConstructorMock = new NotifyPropertyChangedSynchronizeConstructorMock();

            var SourceSynchronizerRoot = new SourceSynchronizerRoot(sourceConstructorMock);

            var TargetSynchronizerRoot =
                new TargetSynchronizerRoot<NotifyPropertyChangedSynchronizeConstructorMock>(SourceSynchronizerRoot.WriteFullAndDispose());

            TargetSynchronizerRoot.Read(SourceSynchronizerRoot.WriteChangesAndDispose().SetTick(0));

            NotifyPropertyChangedSynchronizeConstructorMock targetConstructorMock = TargetSynchronizerRoot.Reference;

            Assert.True(targetConstructorMock.SyncConstructorCalled);
        }

        [Fact]
        public void Synchronizing_GetterManual_Resolves()
        {
            var sourceConstructorMock = new NotifyPropertyChangedManualGetterOnlyMock();

            var sourceSynchronizerRoot = new SourceSynchronizerRoot(sourceConstructorMock);
            Assert.Equal(5, sourceConstructorMock.SomeValue);

            sourceConstructorMock.SomeValue = 6;

            var TargetSynchronizerRoot = new TargetSynchronizerRoot<NotifyPropertyChangedManualGetterOnlyMock>(sourceSynchronizerRoot.WriteFullAndDispose());

            NotifyPropertyChangedManualGetterOnlyMock targetConstructorMock = TargetSynchronizerRoot.Reference;

            Assert.Equal(6, targetConstructorMock.SomeValue);
        }
    }
}