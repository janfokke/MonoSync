using System;
using MonoSync.Attributes;
using MonoSync.Exceptions;
using MonoSync.Test.TestObjects;
using MonoSync.Test.TestUtils;
using Xunit;

namespace MonoSync.Test.Synchronization
{
    public class PlayerMock
    {
        [Synchronize]
        public int Level { get; set; }
    }

    public class ObjectSyncTargetTests
    {
        [Fact]
        public void Synchronizing_Test()
        {
            var playerMock = new PlayerMock();
            playerMock.Level = 5;

            var SourceSynchronizerRoot = new SourceSynchronizerRoot(playerMock);

            var TargetSynchronizerRoot = new TargetSynchronizerRoot<PlayerMock>(SourceSynchronizerRoot.WriteFullAndDispose());

            Assert.Equal(5, TargetSynchronizerRoot.Reference.Level);


        }
    }
}