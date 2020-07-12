using System;
using System.Linq;
using MonoSync.Test.TestObjects;
using MonoSync.Test.TestUtils;
using Xunit;

namespace MonoSync.Test.Synchronization
{
    public class SourceSynchronizerRootTests
    {
        [Fact]
        public void SettingReferenceToNull_ThatIsCyclic_WillNotBeUntracked()
        {
            var selfReferencingChild = new ReferencingCircleHelper();
            selfReferencingChild.Other = selfReferencingChild;

            var referenceToChild = new ReferencingCircleHelper {Other = selfReferencingChild};

            var SourceSynchronizerRoot = new SourceSynchronizerRoot(referenceToChild);

            Assert.Equal(2, SourceSynchronizerRoot.TrackedObjects.Count());

            referenceToChild.Other = null;

            Assert.Equal(2, SourceSynchronizerRoot.TrackedObjects.Count());
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

            var SourceSynchronizerRoot = new SourceSynchronizerRoot(referencingCircleHelper);

            // Write changes to remove reference from pending tracked objects
            SourceSynchronizerRoot.WriteChangesAndDispose();

            referencingCircleHelper.Other = null;

            // Write changes to remove reference from dirty objects
            SourceSynchronizerRoot.WriteChangesAndDispose();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.Equal(1, SourceSynchronizerRoot.PendingUntrackedObjectCount);
        }
    }
}