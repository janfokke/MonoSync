using MonoSync.Exceptions;
using MonoSync.Test.TestObjects;
using MonoSync.Test.TestUtils;
using Xunit;

namespace MonoSync.Test.Synchronization
{
    public class NotifyPropertyChangedSyncSourceTests
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
    }
}