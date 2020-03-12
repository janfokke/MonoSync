using System.Reflection;

namespace MonoSync.Exceptions
{
    public class SetterNotFoundException : MonoSyncException
    {
        public SetterNotFoundException(PropertyInfo propertyInfo) : base(
            $"{propertyInfo.DeclaringType}:{propertyInfo.Name} doesn't have a setter")
        {
        }

        public SetterNotFoundException() : base("Property doesn't have a setter")
        {
        }
    }
}