namespace MonoSync.Exceptions
{
    public class GetterNotFoundException : MonoSyncException
    {
        public GetterNotFoundException(string propertyName) : base($"Property {propertyName} doesn't have a getter")
        {
        }
    }
}