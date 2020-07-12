using System.Collections.Generic;
using System.Linq;
using MonoSync.Collections;
using Xunit;

namespace MonoSync.Test.Synchronization
{
    public class ObservableHashSetSyncSourceTests
    {
        [Fact]
        public void AddingItems_ThatAreReferences_ShouldTrackAddedItems()
        {
            var sourceHashSet = new ObservableHashSet<string>();

            var SourceSynchronizerRoot = new SourceSynchronizerRoot(sourceHashSet);
            sourceHashSet.Add("1");
            sourceHashSet.Add("2");

            Assert.Equal(3, SourceSynchronizerRoot.TrackedObjects.Count());
        }

        [Fact]
        public void Initializing_WithItems_ShouldTrackExistingItems()
        {
            var sourceHashSet = new ObservableHashSet<string>()
            {
                "1", "2"
            };

            var SourceSynchronizerRoot = new SourceSynchronizerRoot(sourceHashSet);
            
            Assert.Equal(3, SourceSynchronizerRoot.TrackedObjects.Count());
        }
    }
}