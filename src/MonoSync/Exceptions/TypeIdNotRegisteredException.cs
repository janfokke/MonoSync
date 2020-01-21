namespace MonoSync.Exceptions
{
    public class TypeIdNotRegisteredException : MonoSyncException
    {
        public TypeIdNotRegisteredException(int typeIdentifier) : base(
            $"Type with ReferenceId {typeIdentifier} is not registered")
        {
        }
    }
}