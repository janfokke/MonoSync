using MonoSync.Collections;
using MonoSync.Test.TestObjects;
using MonoSync.Test.TestUtils;
using Xunit;

namespace MonoSync.Test.Synchronization
{
    public class StringSyncTargetTests
    {
        public StringSyncTargetTests()
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
        public void SynchronizingString_ThatIsNotReferenceEqual_Test()
        {
            var sourceGameWorld = new TestGameWorld { RandomIntProperty = 5 };
            // Concatenating 1 to make sure the string is a new object
            sourceGameWorld.Players.Add("player1", new TestPlayer { Name = "sameString"+1, Health = 100, Level = 30 });
            
            var syncSourceRoot = new SyncSourceRoot(sourceGameWorld, _sourceSettings);

            var syncTargetRoot = new SyncTargetRoot<TestGameWorld>(syncSourceRoot.WriteFullAndDispose(), _targetSettings);

            sourceGameWorld.Players.Add("player2", new TestPlayer { Name = "sameString"+1, Health = 44, Level = 1337 });
            SynchronizationPacket writeChangesAndDispose = syncSourceRoot.WriteChangesAndDispose();
            syncTargetRoot.Read(writeChangesAndDispose.SetTick(0));

            TestGameWorld targetGameWorld = syncTargetRoot.Root;

            AssertExtension.AssertCloneEqual(sourceGameWorld, targetGameWorld);
        }
    }
}