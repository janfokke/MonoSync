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
    public class SyncSourceRootTests
    {
        public SyncSourceRootTests()
        {
            var typeEncoder = new TypeEncoder();
            typeEncoder.RegisterType<TestGameWorld>(StartingIndexNonReservedTypes);
            typeEncoder.RegisterType<TestPlayer>(StartingIndexNonReservedTypes + 1);
            typeEncoder.RegisterType<ReferencingCircleHelper>(StartingIndexNonReservedTypes + 2);

            _sourceSettings = SyncSourceSettings.Default;
            _sourceSettings.TypeEncoder = typeEncoder;

            _targetSettings = SyncTargetSettings.Default;
            _targetSettings.TypeEncoder = typeEncoder;
        }

        private readonly SyncSourceSettings _sourceSettings;
        private readonly SyncTargetSettings _targetSettings;

        [Fact]
        public void ReferenceCycleShouldBeCollectedByGarbageCollectionTest()
        {
            var selfReferencingChild = new ReferencingCircleHelper();
            selfReferencingChild.Other = selfReferencingChild;

            var referenceToChild = new ReferencingCircleHelper {Other = selfReferencingChild};

            var syncSourceRoot = new SyncSourceRoot(referenceToChild, _sourceSettings);

            Assert.Equal(2, syncSourceRoot.TrackedReferences.Count());

            referenceToChild.Other = null;

            syncSourceRoot.GarbageCollect();

            syncSourceRoot.WriteChangesAndDispose();

            Assert.Single(syncSourceRoot.TrackedReferences);
        }

        [Fact]
        public void ReferenceCycleShouldNotReferenceCountCollectTest()
        {
            var selfReferencingChild = new ReferencingCircleHelper();
            selfReferencingChild.Other = selfReferencingChild;

            var referenceToChild = new ReferencingCircleHelper {Other = selfReferencingChild};

            var syncSourceRoot = new SyncSourceRoot(referenceToChild, _sourceSettings);

            Assert.Equal(2, syncSourceRoot.TrackedReferences.Count());

            referenceToChild.Other = null;

            Assert.Equal(2, syncSourceRoot.TrackedReferences.Count());
        }

        [Fact]
        public void SyncRemovedAndChangedTest()
        {
            var world = new TestGameWorld();
            var player = new TestPlayer();
            world.Players.Add("player", player);
            var syncSourceRoot = new SyncSourceRoot(world, _sourceSettings);
            
            var syncTargetRoot = new SyncTargetRoot(syncSourceRoot.WriteFullAndDispose(), _targetSettings);

            player.Health = 3;

            world.Players.Remove("player");

            syncTargetRoot.Read(syncSourceRoot.WriteChangesAndDispose().SetTick(0));
        }

        [Fact]
        public void RemovingReferenceShouldReferenceCountCollectTest()
        {
            var observableDictionary = new ObservableDictionary<int, string>();
            var syncSourceRoot = new SyncSourceRoot(observableDictionary, _sourceSettings);
            observableDictionary.Add(1, "1");

            syncSourceRoot.BeginWrite().Dispose();

            Assert.Equal(2, syncSourceRoot.TrackedReferences.Count());

            observableDictionary.Remove(1);

            // Object are removed after write
            syncSourceRoot.WriteChangesAndDispose();

            Assert.Single(syncSourceRoot.TrackedReferences);
        }

        [Fact]
        public void WriteChangesShouldNotDoubleSynchronizeTest()
        {
            var sourceGameWorld = new TestGameWorld {RandomIntProperty = 5};

            var syncSourceRoot = new SyncSourceRoot(sourceGameWorld, _sourceSettings);


            var syncTargetRoot =
                new SyncTargetRoot<TestGameWorld>(syncSourceRoot.WriteFullAndDispose(), _targetSettings);

            TestGameWorld previousTargetTestGameWorld = syncTargetRoot.Root;


            syncTargetRoot.Read(syncSourceRoot.WriteChangesAndDispose().SetTick(0));

            Assert.Equal(previousTargetTestGameWorld, syncTargetRoot.Root);
        }
    }
}