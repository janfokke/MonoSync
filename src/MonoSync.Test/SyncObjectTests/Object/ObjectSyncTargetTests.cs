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
            var sourceSynchronizerRoot = new SourceSynchronizerRoot(playerMock);
            var targetSynchronizerRoot = new TargetSynchronizerRoot<PlayerMock>(sourceSynchronizerRoot.WriteFullAndDispose());

            Assert.Equal(5, targetSynchronizerRoot.Reference.Level);
        }

        [Fact]
        public void Synchronizing_PrivateField_Test()
        {
            var privateMock = new PrivateFieldMock(5);
            var SourceSynchronizerRoot = new SourceSynchronizerRoot(privateMock);
            var TargetSynchronizerRoot = new TargetSynchronizerRoot<PrivateFieldMock>(SourceSynchronizerRoot.WriteFullAndDispose());
            Assert.Equal(5, TargetSynchronizerRoot.Reference.GetTestValue);
        }

        [Fact]
        public void Synchronizing_Array_Test()
        {
            int[,,] expected = new int[3, 3, 5]
            {
                {{1, 2, 6, 4, 5}, {2, 2, 3, 4, 5}, {3, 2, 3, 4, 5}},
                {{1, 2, 3, 4, 5}, {2, 2, 3, 4, 5}, {3, 2, 3, 4, 5}},
                {{1, 2, 3, 4, 5}, {99, 2, 3, 4, 5}, {3, 2, 3, 99, 3}}
            };
            var SourceSynchronizerRoot = new SourceSynchronizerRoot(expected);
            var TargetSynchronizerRoot = new TargetSynchronizerRoot<int[,,]>(SourceSynchronizerRoot.WriteFullAndDispose());
            Assert.Equal(expected, TargetSynchronizerRoot.Reference);
        }
    }
}