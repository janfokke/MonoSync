namespace MonoSync.Exceptions
{
    public class ReferenceIsNotTrackedException : MonoSyncException
    {
        public object Reference { get; }

        public ReferenceIsNotTrackedException(object reference) : base($"Reference {reference} is not tracked")
        {
            Reference = reference;
        }
    }
}