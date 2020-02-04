using MonoSync.Attributes;

namespace MonoSync.Exceptions
{
    public class SynchronizedMarkedMethodParameterException : MonoSyncException
    {
        public SynchronizedMarkedMethodParameterException() : base(
            $"Methods marked with the {nameof(OnSynchronizedAttribute)} cannot have parameters")
        {
        }
    }
}