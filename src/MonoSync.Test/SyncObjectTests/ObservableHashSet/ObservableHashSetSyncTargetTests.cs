using System.Linq;
using MonoSync.Collections;
using MonoSync.Test.TestObjects;
using MonoSync.Test.TestUtils;
using Xunit;

namespace MonoSync.Test.Synchronization
{
    public class ObservableHashSetSyncTargetTests
    {
        public ObservableHashSetSyncTargetTests()
        {
            var typeEncoder = new TypeEncoder();
            typeEncoder.RegisterType<HashSetTestObject>(TypeEncoder.ReservedIdentifiers.StartingIndexNonReservedTypes);
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
            var source = new ObservableHashSet<string>();
            var syncSourceRoot = new SyncSourceRoot(source, _sourceSettings);

            byte[] writeFullAndDispose = syncSourceRoot.WriteFullAndDispose();
            var syncTargetRoot = new SyncTargetRoot<ObservableHashSet<string>>(
                writeFullAndDispose,
                _targetSettings);

            ObservableHashSet<string> target = syncTargetRoot.Root;
            target.Add("2");

            syncTargetRoot.Clock.OwnTick = 5;

            //Set tick older than client tick
            byte[] changes = syncSourceRoot.WriteChangesAndDispose().SetTick(6);
            syncTargetRoot.Read(changes);

            // Recently added item should be rolled back
            Assert.Empty(target);
        }

        [Fact]
        public void SynchronizingFull_TargetObjectEqualsSource()
        {
            var sourceGameWorld = new HashSetTestObject { RandomIntProperty = 5};
            sourceGameWorld.Players.Add(new TestPlayer {Name = "player1", Health = 100, Level = 30 });
            sourceGameWorld.Players.Add(new TestPlayer {Name = "player2", Health = 44, Level = 1337 });

            var syncSourceRoot = new SyncSourceRoot(sourceGameWorld, _sourceSettings);

            var syncTargetRoot = new SyncTargetRoot<HashSetTestObject>(syncSourceRoot.WriteFullAndDispose(), _targetSettings);
            HashSetTestObject targetGameWorld = syncTargetRoot.Root;

            AssertExtension.AssertCloneEqual(sourceGameWorld, targetGameWorld);
        }

        [Fact]
        public void AddingItems_AfterClear_ShouldSynchronizeItems()
        {
            var hashSetTestObject = new HashSetTestObject { RandomIntProperty = 5 };
            
            var syncSourceRoot = new SyncSourceRoot(hashSetTestObject, _sourceSettings);

            hashSetTestObject.Players.Clear();

            var syncTargetRoot = new SyncTargetRoot<HashSetTestObject>(syncSourceRoot.WriteFullAndDispose(), _targetSettings);

            hashSetTestObject.Players.Add(new TestPlayer { Name = "player1", Health = 100, Level = 30 });
            hashSetTestObject.Players.Add(new TestPlayer { Name = "player2", Health = 44, Level = 1337 });

            syncTargetRoot.Read(syncSourceRoot.WriteChangesAndDispose().SetTick(10));

            HashSetTestObject targetGameWorld = syncTargetRoot.Root;

            AssertExtension.AssertCloneEqual(hashSetTestObject, targetGameWorld);
        }
    }
}