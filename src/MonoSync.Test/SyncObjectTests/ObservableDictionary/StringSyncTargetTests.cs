using MonoSync.Collections;
using MonoSync.Test.TestObjects;
using MonoSync.Test.TestUtils;
using Xunit;

namespace MonoSync.Test.Synchronization
{
    public class StringSyncTargetTests
    {
        [Fact]
        public void SynchronizingString_ThatIsNotReferenceEqual_Test()
        {
            var sourceGameWorld = new TestGameWorld { RandomIntProperty = 5 };
            // Concatenating 1 to make sure the string is a new object
            sourceGameWorld.Players.Add("player1", new TestPlayer { Name = "sameString"+1, Health = 100, Level = 30 });
            
            var SourceSynchronizerRoot = new SourceSynchronizerRoot(sourceGameWorld);

            var TargetSynchronizerRoot = new TargetSynchronizerRoot<TestGameWorld>(SourceSynchronizerRoot.WriteFullAndDispose());

            sourceGameWorld.Players.Add("player2", new TestPlayer { Name = "sameString"+1, Health = 44, Level = 1337 });
            SynchronizationPacket writeChangesAndDispose = SourceSynchronizerRoot.WriteChangesAndDispose();
            TargetSynchronizerRoot.Read(writeChangesAndDispose.SetTick(0));

            TestGameWorld targetGameWorld = TargetSynchronizerRoot.Reference;

            AssertExtension.AssertCloneEqual(sourceGameWorld, targetGameWorld);
        }
    }
}