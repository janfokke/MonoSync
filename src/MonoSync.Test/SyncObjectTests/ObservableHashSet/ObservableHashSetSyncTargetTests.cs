using System.Linq;
using MonoSync.Collections;
using MonoSync.Test.TestObjects;
using MonoSync.Test.TestUtils;
using Xunit;

namespace MonoSync.Test.Synchronization
{
    public class ObservableHashSetSyncTargetTests
    {
        [Fact]
        public void Synchronizing_RollsBackTargetChangesPriorToSourceTick()
        {
            var source = new ObservableHashSet<string>();
            var SourceSynchronizerRoot = new SourceSynchronizerRoot(source);

            byte[] writeFullAndDispose = SourceSynchronizerRoot.WriteFullAndDispose();
            var TargetSynchronizerRoot = new TargetSynchronizerRoot<ObservableHashSet<string>>(
                writeFullAndDispose);

            ObservableHashSet<string> target = TargetSynchronizerRoot.Reference;
            target.Add("2");

            TargetSynchronizerRoot.Clock.OwnTick = 5;

            //Set tick older than client tick
            byte[] changes = SourceSynchronizerRoot.WriteChangesAndDispose().SetTick(6);
            TargetSynchronizerRoot.Read(changes);

            // Recently added item should be rolled back
            Assert.Empty(target);
        }

        [Fact]
        public void SynchronizingFull_TargetObjectEqualsSource()
        {
            var sourceGameWorld = new NotifyPropertyChangedHashSetTestObject { RandomIntProperty = 5};
            sourceGameWorld.Players.Add(new NotifyPropertyChangedTestPlayer {Name = "player1", Health = 100, Level = 30 });
            sourceGameWorld.Players.Add(new NotifyPropertyChangedTestPlayer {Name = "player2", Health = 44, Level = 1337 });

            var SourceSynchronizerRoot = new SourceSynchronizerRoot(sourceGameWorld);

            var TargetSynchronizerRoot = new TargetSynchronizerRoot<NotifyPropertyChangedHashSetTestObject>(SourceSynchronizerRoot.WriteFullAndDispose());
            NotifyPropertyChangedHashSetTestObject targetGameWorld = TargetSynchronizerRoot.Reference;

            AssertExtension.AssertCloneEqual(sourceGameWorld, targetGameWorld);
        }

        [Fact]
        public void AddingItems_AfterClear_ShouldSynchronizeItems()
        {
            var hashSetTestObject = new NotifyPropertyChangedHashSetTestObject { RandomIntProperty = 5 };
            
            var SourceSynchronizerRoot = new SourceSynchronizerRoot(hashSetTestObject);

            hashSetTestObject.Players.Clear();

            var TargetSynchronizerRoot = new TargetSynchronizerRoot<NotifyPropertyChangedHashSetTestObject>(SourceSynchronizerRoot.WriteFullAndDispose());

            hashSetTestObject.Players.Add(new NotifyPropertyChangedTestPlayer { Name = "player1", Health = 100, Level = 30 });
            hashSetTestObject.Players.Add(new NotifyPropertyChangedTestPlayer { Name = "player2", Health = 44, Level = 1337 });

            TargetSynchronizerRoot.Read(SourceSynchronizerRoot.WriteChangesAndDispose().SetTick(10));

            NotifyPropertyChangedHashSetTestObject targetGameWorld = TargetSynchronizerRoot.Reference;

            AssertExtension.AssertCloneEqual(hashSetTestObject, targetGameWorld);
        }
    }
}