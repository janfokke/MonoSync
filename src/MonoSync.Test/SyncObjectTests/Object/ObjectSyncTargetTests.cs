using System;
using MonoSync.Exceptions;
using MonoSync.Test.TestObjects;
using MonoSync.Test.TestUtils;
using Xunit;

namespace MonoSync.Test.Synchronization
{
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

        [Fact]
        public void Synchronizing_PrivateField_Test()
        {
            var privateMock = new PrivateFieldMock(5);
            var SourceSynchronizerRoot = new SourceSynchronizerRoot(privateMock);
            var TargetSynchronizerRoot = new TargetSynchronizerRoot<PrivateFieldMock>(SourceSynchronizerRoot.WriteFullAndDispose());
            Assert.Equal(5, TargetSynchronizerRoot.Reference.GetTestValue);
        }
    }
}