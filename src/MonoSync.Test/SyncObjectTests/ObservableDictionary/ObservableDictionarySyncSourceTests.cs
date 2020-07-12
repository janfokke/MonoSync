using System;
using System.Collections.Generic;
using System.Linq;
using MonoSync.Collections;
using MonoSync.Test.TestUtils;
using Xunit;

namespace MonoSync.Test.Synchronization
{
    public class ObservableDictionarySyncSourceTests
    {
        [Fact]
        public void Clear_ShouldThrowNotImplemented()
        {
            var sourceDictionary = new ObservableDictionary<int, string>();
            var SourceSynchronizerRoot = new SourceSynchronizerRoot(sourceDictionary);
            byte[] data = SourceSynchronizerRoot.WriteFullAndDispose();
            var TargetSynchronizerRoot = new TargetSynchronizerRoot<ObservableDictionary<int, string>>(data);
            Assert.Throws<NotSupportedException>(() => { TargetSynchronizerRoot.Reference.Clear(); });
        }

        [Fact]
        public void AddingItems_ThatAreReferences_ShouldTrackAddedItems()
        {
            var sourceDictionary = new ObservableDictionary<int, string>();

            var SourceSynchronizerRoot = new SourceSynchronizerRoot(sourceDictionary);
            sourceDictionary.Add(1, "1");
            sourceDictionary.Add(2, "2");

            Assert.Equal(3, SourceSynchronizerRoot.TrackedObjects.Count());
        }

        [Fact]
        public void Initializing_WithItems_ShouldTrackExistingItems()
        {
            var sourceDictionary = new ObservableDictionary<int, string>
            {
                { 1, "1" },
                { 2, "2" }
            };

            var SourceSynchronizerRoot = new SourceSynchronizerRoot(sourceDictionary);

            Assert.Equal(3, SourceSynchronizerRoot.TrackedObjects.Count());
        }
    }
}