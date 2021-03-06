using System;
using System.Linq;
using MonoSync.Collections;
using MonoSync.Test.TestObjects;
using MonoSync.Test.TestUtils;
using Xunit;

namespace MonoSync.Test.Synchronization
{
    public class ObservableDictionarySyncTargetTests
    {
        [Fact]
        public void Synchronizing_RollsBackTargetChangesPriorToSourceTick()
        {
            var sourceDictionary = new ObservableDictionary<int, string>();

            var sourceSynchronizerRoot = new SourceSynchronizerRoot(sourceDictionary);

            var targetSynchronizerRoot = new TargetSynchronizerRoot<ObservableDictionary<int, string>>(
                sourceSynchronizerRoot.WriteFullAndDispose());

            ObservableDictionary<int, string> targetDictionary = targetSynchronizerRoot.Reference;
            targetDictionary.Add(1, "2");

            targetSynchronizerRoot.Clock.OwnTick = TimeSpan.FromMilliseconds(5);

            //Set tick older than client tick
            byte[] changes = sourceSynchronizerRoot.WriteChangesAndDispose().SetTick(TimeSpan.FromMilliseconds(5));
            targetSynchronizerRoot.Read(changes);

            // Recently added item should be rolled back
            Assert.Empty(targetDictionary);
        }

        [Fact]
        public void SynchronizingFull_TargetObjectEqualsSource()
        {
            var sourceGameWorld = new NotifyPropertyChangedTestGameWorld {RandomIntProperty = 5};
            sourceGameWorld.Players.Add("player1", new NotifyPropertyChangedTestPlayer {Name = "player1", Health = 100, Level = 30 });
            sourceGameWorld.Players.Add("player2", new NotifyPropertyChangedTestPlayer {Name = "player2", Health = 44, Level = 1337 });

            var sourceSynchronizerRoot = new SourceSynchronizerRoot(sourceGameWorld);

            var targetSynchronizerRoot = new TargetSynchronizerRoot<NotifyPropertyChangedTestGameWorld>(sourceSynchronizerRoot.WriteFullAndDispose());
            NotifyPropertyChangedTestGameWorld targetGameWorld = targetSynchronizerRoot.Reference;

            AssertExtension.AssertCloneEqual(sourceGameWorld, targetGameWorld);
        }

        [Fact]
        public void AddingItems_AfterClear_ShouldSynchronizeItems()
        {
            var sourceGameWorld = new NotifyPropertyChangedTestGameWorld { RandomIntProperty = 5 };
            
            var sourceSynchronizerRoot = new SourceSynchronizerRoot(sourceGameWorld);

            sourceGameWorld.Players.Clear();

            var targetSynchronizerRoot = new TargetSynchronizerRoot<NotifyPropertyChangedTestGameWorld>(sourceSynchronizerRoot.WriteFullAndDispose());

            sourceGameWorld.Players.Add("player1", new NotifyPropertyChangedTestPlayer { Name = "player1", Health = 100, Level = 30 });
            sourceGameWorld.Players.Add("player2", new NotifyPropertyChangedTestPlayer { Name = "player2", Health = 44, Level = 1337 });

            targetSynchronizerRoot.Read(sourceSynchronizerRoot.WriteChangesAndDispose().SetTick(TimeSpan.FromMilliseconds(10)));

            NotifyPropertyChangedTestGameWorld targetGameWorld = targetSynchronizerRoot.Reference;

            AssertExtension.AssertCloneEqual(sourceGameWorld, targetGameWorld);
        }
    }
}