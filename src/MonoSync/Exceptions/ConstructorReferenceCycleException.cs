using System.Collections.Generic;
using System.Linq;

namespace MonoSync.Exceptions
{
    public class ConstructorReferenceCycleException : MonoSyncException
    {
        public List<object> Path { get; }

        public ConstructorReferenceCycleException(List<object> path) : base(
            $"Constructor loop detected: {string.Join(' ', path.Select(p => p.GetType().Name))}")
        {
            Path = path;
        }
    }
}