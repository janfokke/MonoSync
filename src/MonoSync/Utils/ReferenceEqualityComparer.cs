using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MonoSync.Utils
{
    public sealed class ReferenceEqualityComparer
        : IEqualityComparer, IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Default = new ReferenceEqualityComparer();

        private ReferenceEqualityComparer() { }

#pragma warning disable 108,114
        public bool Equals(object x, object y)
#pragma warning restore 108,114
        {
            return x == y;
        }

        public int GetHashCode(object obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}