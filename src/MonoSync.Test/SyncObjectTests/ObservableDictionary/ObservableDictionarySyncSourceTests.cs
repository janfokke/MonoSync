using System.Collections.Generic;
using System.Linq;
using MonoSync.Collections;
using Xunit;

namespace MonoSync.Test.Synchronization
{
    public class ObservableDictionarySyncSourceTests
    {
        [Fact]
        public void AddingItems_ThatAreReferences_ShouldTrackAddedItems()
        {
            var sourceDictionary = new ObservableDictionary<int, string>();

            var syncSourceRoot = new SyncSourceRoot(sourceDictionary, SyncSourceSettings.Default);
            sourceDictionary.Add(1, "1");
            sourceDictionary.Add(2, "2");

            Assert.Equal(3, syncSourceRoot.TrackedObjects.Count());
        }

        [Fact]
        public void Initializing_WithItems_ShouldTrackExistingItems()
        {
            var sourceDictionary = new ObservableDictionary<int, string>();
            sourceDictionary.Add(1, "1");
            sourceDictionary.Add(2, "2");

            var syncSourceRoot = new SyncSourceRoot(sourceDictionary, SyncSourceSettings.Default);

            Assert.Equal(3, syncSourceRoot.TrackedObjects.Count());
        }
    }
}