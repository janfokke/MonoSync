namespace MonoSync.Exceptions
{
    public class GetterNotFoundException : MonoSyncException
    {
        public GetterNotFoundException(string propertyName) : base($"Value {propertyName} doesn't have a getter")
        {
        }
    }
}