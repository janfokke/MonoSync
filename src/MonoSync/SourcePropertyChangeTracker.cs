using System;
using System.Collections;
using System.Collections.Generic;

namespace MonoSync
{
    /// <summary>
    ///     Properties should be sorted. That's why OrderedDictionary is used
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    public class SourcePropertyChangeTracker<TProperty> : IEnumerable<TProperty> where TProperty : SyncProperty
    {
        private readonly SortedDictionary<int, TProperty> _changedProperties = new SortedDictionary<int, TProperty>();

        public IEnumerator<TProperty> GetEnumerator()
        {
            return _changedProperties.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public event EventHandler Dirty;

        public void MarkDirty(TProperty value)
        {
            if (_changedProperties.ContainsKey(value.Index) == false)
            {
                _changedProperties.Add(value.Index, value);
                if (_changedProperties.Count == 1) OnDirty();
            }
        }

        public void Clear()
        {
            _changedProperties.Clear();
        }

        protected virtual void OnDirty()
        {
            Dirty?.Invoke(this, EventArgs.Empty);
        }
    }
}