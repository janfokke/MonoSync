using System;
using System.Linq;
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
        public void SettingReferenceToNull_ThatIsCyclic_WillNotBeUntracked()
        {
            var selfReferencingChild = new ReferencingCircleHelper();
            selfReferencingChild.Other = selfReferencingChild;

            var referenceToChild = new ReferencingCircleHelper {Other = selfReferencingChild};

            var syncSourceRoot = new SyncSourceRoot(referenceToChild, _sourceSettings);

            Assert.Equal(2, syncSourceRoot.TrackedObjects.Count());

            referenceToChild.Other = null;

            Assert.Equal(2, syncSourceRoot.TrackedObjects.Count());
        }

        [Fact]
        public void GarbageCollection_UntracksObjectThatHaveNoReferences()
        {
            // Local function to avoid reference from stack when garbage collector runs
            static ReferencingCircleHelper ReferencingCircleHelper()
            {
                var selfReferencingObject = new ReferencingCircleHelper();
                selfReferencingObject.Other = selfReferencingObject;

                var otherReferencingObject = new ReferencingCircleHelper {Other = selfReferencingObject};
                return otherReferencingObject;
            }

            ReferencingCircleHelper referencingCircleHelper = ReferencingCircleHelper();

            var syncSourceRoot = new SyncSourceRoot(referencingCircleHelper, _sourceSettings);

            // Write changes to remove reference from pending tracked objects
            syncSourceRoot.WriteChangesAndDispose();

            referencingCircleHelper.Other = null;

            // Write changes to remove reference from dirty objects
            syncSourceRoot.WriteChangesAndDispose();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.Equal(1, syncSourceRoot.PendingUntrackedObjectCount);
        }
    }
}