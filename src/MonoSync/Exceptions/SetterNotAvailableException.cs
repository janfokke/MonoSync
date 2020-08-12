using System.Reflection;

namespace MonoSync.Exceptions
{
    public class SetterNotAvailableException : MonoSyncException
    {
        public SetterNotAvailableException(MemberInfo memberInfo) : base(
            $"{memberInfo.DeclaringType}:{memberInfo.Name} doesn't have a setter")
        {
        }
    }
}