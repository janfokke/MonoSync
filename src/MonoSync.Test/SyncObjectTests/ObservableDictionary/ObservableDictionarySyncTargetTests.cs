using System.Linq;
using MonoSync.Collections;
using MonoSync.Test.TestObjects;
using MonoSync.Test.TestUtils;
using Xunit;

namespace MonoSync.Test.Synchronization
{
    public class ObservableDictionarySyncTargetTests
    {
        public ObservableDictionarySyncTargetTests()
        {
            var typeEncoder = new TypeEncoder();
            typeEncoder.RegisterType<TestGameWorld>(TypeEncoder.ReservedIdentifiers.StartingIndexNonReservedTypes);
            typeEncoder.RegisterType<TestPlayer>(TypeEncoder.ReservedIdentifiers.StartingIndexNonReservedTypes + 1);

            _sourceSettings = SyncSourceSettings.Default;
            _sourceSettings.TypeEncoder = typeEncoder;

            _targetSettings = SyncTargetSettings.Default;
            _targetSettings.TypeEncoder = typeEncoder;
        }

        private readonly SyncTargetSettings _targetSettings;
        private readonly SyncSourceSettings _sourceSettings;

        [Fact]
        public void Synchronizing_RollsBackTargetChangesPriorToSourceTick()
        {
            var sourceDictionary = new ObservableDictionary<int, string>();

            var syncSourceRoot = new SyncSourceRoot(sourceDictionary, _sourceSettings);

            var syncTargetRoot = new SyncTargetRoot<ObservableDictionary<int, string>>(
                syncSourceRoot.WriteFullAndDispose(),
                _targetSettings);

            ObservableDictionary<int, string> targetDictionary = syncTargetRoot.Root;
            targetDictionary.Add(1, "2");

            syncTargetRoot.Clock.OwnTick = 5;

            //Set tick older than client tick
            byte[] changes = syncSourceRoot.WriteChangesAndDispose().SetTick(6);
            syncTargetRoot.Read(changes);

            // Recently added item should be rolled back
            Assert.Empty(targetDictionary);
        }

        [Fact]
        public void SynchronizingFull_TargetObjectEqualsSource()
        {
            var sourceGameWorld = new TestGameWorld {RandomIntProperty = 5};
            sourceGameWorld.Players.Add("player1", new TestPlayer { Health = 100, Level = 30 });
            sourceGameWorld.Players.Add("player2", new TestPlayer { Health = 44, Level = 1337 });

            var syncSourceRoot = new SyncSourceRoot(sourceGameWorld, _sourceSettings);

            var syncTargetRoot = new SyncTargetRoot<TestGameWorld>(syncSourceRoot.WriteFullAndDispose(), _targetSettings);
            TestGameWorld targetGameWorld = syncTargetRoot.Root;

            AssertExtension.AssertCloneEqual(sourceGameWorld, targetGameWorld);
        }

        [Fact]
        public void AddingItems_AfterClear_ShouldSynchronizeItems()
        {
            var sourceGameWorld = new TestGameWorld { RandomIntProperty = 5 };
            
            var syncSourceRoot = new SyncSourceRoot(sourceGameWorld, _sourceSettings);

            sourceGameWorld.Players.Clear();

            var syncTargetRoot = new SyncTargetRoot<TestGameWorld>(syncSourceRoot.WriteFullAndDispose(), _targetSettings);

            sourceGameWorld.Players.Add("player1", new TestPlayer { Health = 100, Level = 30 });
            sourceGameWorld.Players.Add("player2", new TestPlayer { Health = 44, Level = 1337 });

            syncTargetRoot.Read(syncSourceRoot.WriteChangesAndDispose().SetTick(10));

            TestGameWorld targetGameWorld = syncTargetRoot.Root;

            AssertExtension.AssertCloneEqual(sourceGameWorld, targetGameWorld);
        }
    }
}