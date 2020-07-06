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

            var syncSourceRoot = new SyncSourceRoot(sourceHashSet, SyncSourceSettings.Default);
            sourceHashSet.Add("1");
            sourceHashSet.Add("2");

            Assert.Equal(3, syncSourceRoot.TrackedObjects.Count());
        }

        [Fact]
        public void Initializing_WithItems_ShouldTrackExistingItems()
        {
            var sourceHashSet = new ObservableHashSet<string>()
            {
                "1", "2"
            };

            var syncSourceRoot = new SyncSourceRoot(sourceHashSet, SyncSourceSettings.Default);
            
            Assert.Equal(3, syncSourceRoot.TrackedObjects.Count());
        }
    }
}