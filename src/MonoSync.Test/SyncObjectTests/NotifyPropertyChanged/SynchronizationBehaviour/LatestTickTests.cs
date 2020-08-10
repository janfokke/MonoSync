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
        [Fact]
        public void Synchronizing_ValueWithLowerTickThanDirtyTick_ShouldNotBeSet()
        {
            var sourceObject = new NotifyPropertyChangedLatestTickMock { Value = 5 };
            var SourceSynchronizerRoot = new SourceSynchronizerRoot(sourceObject);
            
            var TargetSynchronizerRoot = new TargetSynchronizerRoot<NotifyPropertyChangedLatestTickMock>(SourceSynchronizerRoot.WriteFullAndDispose());
            var targetObject = TargetSynchronizerRoot.Reference;
            Assert.Equal(5, targetObject.Value);

            TargetSynchronizerRoot.Clock.OwnTick = 10;
            targetObject.Value = 7;
            sourceObject.Value = 6;

            TargetSynchronizerRoot.Read(SourceSynchronizerRoot.WriteChangesAndDispose().SetTick(9));

            Assert.Equal(7,targetObject.Value);

            TargetSynchronizerRoot.Read(SourceSynchronizerRoot.WriteChangesAndDispose().SetTick(11));

            Assert.Equal(6, targetObject.Value);
        }
    }
}
