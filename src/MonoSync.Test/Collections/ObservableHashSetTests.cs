using System;
using MonoSync.Collections;
using Xunit;

namespace MonoSync.Test.Collections
{
    public class ObservableHashSetTests
    {
        [Fact]
        public void Add_ShouldAddAnItem()
        {
            var observableHash = new ObservableHashSet<int>();
            observableHash.Add(1);
            Assert.Contains(1, observableHash);
        }

        [Fact]
        public void Add_RaisesCollectionChanged()
        {
            bool flag = false;
            var observableHash = new ObservableHashSet<int>();
            observableHash.CollectionChanged += (sender, args) => flag = true;
            observableHash.Add(1);
            Assert.True(flag);
        }

        [Fact]
        public void Add_DoesNotRaisesCollectionChanged_WhenAlreadyAdded()
        {
            bool flag = false;
            var observableHash = new ObservableHashSet<int>() {1};
            observableHash.CollectionChanged += (sender, args) => flag = true;
            observableHash.Add(1);
            Assert.False(flag);
        }

        [Fact]
        public void Remove_RaisesCollectionChanged()
        {
            bool flag = false;
            var observableHash = new ObservableHashSet<int>() {1};
            observableHash.CollectionChanged += (sender, args) => flag = true;
            observableHash.Remove(1);
            Assert.True(flag);
        }

        [Fact]
        public void Remove_DoesNotRaisesCollectionChanged_WhenCollectionDoesNotContainItem()
        {
            bool flag = false;
            var observableHash = new ObservableHashSet<int>();
            observableHash.CollectionChanged += (sender, args) => flag = true;
            observableHash.Remove(1);
            Assert.False(flag);
        }
    }
}