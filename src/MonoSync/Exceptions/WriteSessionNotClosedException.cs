namespace MonoSync.Exceptions
{
    public class WriteSessionNotClosedException : MonoSyncException
    {
        public WriteSessionNotClosedException() : base("Previous write session is still open")
        {
        }
    }
}