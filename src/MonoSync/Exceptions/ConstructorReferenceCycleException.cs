using System.Collections.Generic;

namespace MonoSync.Exceptions
{
    public class ConstructorReferenceCycleException : MonoSyncException
    {
        public List<object> Path { get; }

        public ConstructorReferenceCycleException(List<object> path) : base(
            $"Constructor loop detected in {path[0].GetType().Name}")
        {
            Path = path;
        }
    }

    public class DoubleSynchronizedReferenceException : MonoSyncException
    {
        public DoubleSynchronizedReferenceException() : base("Object reference got synchronized twice")
        {
        }
    }
}