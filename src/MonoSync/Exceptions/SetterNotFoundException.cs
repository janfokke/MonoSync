namespace MonoSync.Exceptions
{
    public class SetterNotFoundException : MonoSyncException
    {
        public SetterNotFoundException(string propertyName) : base($"Property {propertyName} doesn't have a setter")
        {
        }
    }
}