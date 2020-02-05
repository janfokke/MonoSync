using System.Collections.Generic;
using System.Linq;
using MonoSync.Collections;
using MonoSync.SyncSource;
using MonoSync.SyncTarget;
using MonoSync.Test.TestObjects;
using MonoSync.Test.TestUtils;
using Xunit;
using static MonoSync.TypeEncoder.ReservedIdentifiers;

namespace MonoSync.Test.Synchronization
{
    public class ObservableDictionaryTests
    {
        public ObservableDictionaryTests()
        {
            var typeEncoder = new TypeEncoder();
            typeEncoder.RegisterType<TestGameWorld>(StartingIndexNonReservedTypes);
            typeEncoder.RegisterType<TestPlayer>(StartingIndexNonReservedTypes + 1);

            _sourceSettings = SyncSourceSettings.Default;
            _sourceSettings.TypeEncoder = typeEncoder;

            _targetSettings = SyncTargetSettings.Default;
            _targetSettings.TypeEncoder = typeEncoder;
        }

        private readonly SyncTargetSettings _targetSettings;
        private readonly SyncSourceSettings _sourceSettings;

        [Fact]
        public void ObservableDictionaryAddItemTest()
        {
            var sourceDictionary = new ObservableDictionary<int, string>();
            sourceDictionary.Add(1, "1");
            sourceDictionary.Add(2, "2");

            // Full write
            var syncSourceRoot = new SyncSourceRoot(sourceDictionary, _sourceSettings);


            var syncTargetRoot =
                new SyncTargetRoot<ObservableDictionary<int, string>>(syncSourceRoot.WriteFullAndDispose(),
                    _targetSettings);
            ObservableDictionary<int, string> targetDictionary = syncTargetRoot.Root;

            foreach (KeyValuePair<int, string> keyValuePair in sourceDictionary)
            {
                Assert.Equal(keyValuePair.Value, targetDictionary[keyValuePair.Key]);
            }

            // Changes

            sourceDictionary.Add(3, "3");


            byte[] changesSynchronization = syncSourceRoot.WriteChangesAndDispose().SetTick(0);

            syncTargetRoot.Read(changesSynchronization);

            foreach (KeyValuePair<int, string> keyValuePair in sourceDictionary)
            {
                Assert.Equal(keyValuePair.Value, targetDictionary[keyValuePair.Key]);
            }
        }

        [Fact]
        public void ObservableDictionaryReferenceTrackOnClearTest()
        {
            var sourceDictionary = new ObservableDictionary<int, string>();

            var syncSourceRoot = new SyncSourceRoot(sourceDictionary, SyncSourceSettings.Default);

            sourceDictionary.Add(1, "1");
            sourceDictionary.Add(2, "2");

            Assert.Equal(3, syncSourceRoot.TrackedReferences.Count());

            sourceDictionary.Clear();

            Assert.Single(syncSourceRoot.AddedReferences);
        }

        [Fact]
        public void ObservableDictionaryReferenceTrackOnPostAddTest()
        {
            var sourceDictionary = new ObservableDictionary<int, string>();

            var syncSourceRoot = new SyncSourceRoot(sourceDictionary, SyncSourceSettings.Default);

            sourceDictionary.Add(1, "1");
            sourceDictionary.Add(2, "2");

            Assert.Equal(3, syncSourceRoot.TrackedReferences.Count());
        }

        [Fact]
        public void ObservableDictionaryReferenceTrackOnPreAddTest()
        {
            var sourceDictionary = new ObservableDictionary<int, string>();
            sourceDictionary.Add(1, "1");
            sourceDictionary.Add(2, "2");

            var syncSourceRoot = new SyncSourceRoot(sourceDictionary, SyncSourceSettings.Default);

            Assert.Equal(3, syncSourceRoot.TrackedReferences.Count());
        }

        [Fact]
        public void ObservableDictionaryReferenceTrackOnRemoveTest()
        {
            var sourceDictionary = new ObservableDictionary<int, string>();

            var syncSourceRoot = new SyncSourceRoot(sourceDictionary, SyncSourceSettings.Default);

            sourceDictionary.Add(1, "1");
            sourceDictionary.Add(2, "2");
            sourceDictionary.Remove(1);

            Assert.Equal(2, syncSourceRoot.TrackedReferences.Count());
        }

        [Fact]
        public void SynchronizeChangesAddedItemTest()
        {
            var sourceGameWorld = new TestGameWorld();
            sourceGameWorld.RandomIntProperty = 5;

            var syncSourceRoot = new SyncSourceRoot(sourceGameWorld, _sourceSettings);

            var syncTargetRoot =
                new SyncTargetRoot<TestGameWorld>(syncSourceRoot.WriteFullAndDispose(), _targetSettings);

            sourceGameWorld.Players.Add("player1", new TestPlayer {Health = 100, Level = 30});
            sourceGameWorld.Players.Add("player2", new TestPlayer {Health = 44, Level = 1337});


            syncTargetRoot.Read(syncSourceRoot.WriteChangesAndDispose().SetTick(0));

            TestGameWorld targetGameWorld = syncTargetRoot.Root;

            AssertExtension.AssertCloneEqual(sourceGameWorld, targetGameWorld);
        }

        [Fact]
        public void SynchronizeFullAddedItemTest()
        {
            var sourceGameWorld = new TestGameWorld();
            sourceGameWorld.RandomIntProperty = 5;
            sourceGameWorld.Players.Add("player1", new TestPlayer {Health = 100, Level = 30});
            sourceGameWorld.Players.Add("player2", new TestPlayer {Health = 44, Level = 1337});

            var syncSourceRoot = new SyncSourceRoot(sourceGameWorld, _sourceSettings);


            var syncTargetRoot = new SyncTargetRoot<TestGameWorld>(syncSourceRoot.WriteFullAndDispose(), _targetSettings);
            TestGameWorld targetGameWorld = syncTargetRoot.Root;

            AssertExtension.AssertCloneEqual(sourceGameWorld, targetGameWorld);
        }
    }
}