namespace MonoSync.Exceptions
{
    public class WriteSessionNotOpenException : MonoSyncException
    {
        public WriteSessionNotOpenException() : base("No write session is opened")
        {
        }
    }
}