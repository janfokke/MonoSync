using MonoSync.Attributes;

namespace MonoSync.Exceptions
{
    public class ParameterizedSynchronizedCallbackException : MonoSyncException
    {
        public ParameterizedSynchronizedCallbackException() : base(
            $"Methods marked with the {nameof(OnSynchronizedAttribute)} cannot have parameters")
        {
        }
    }
}