using System.Reflection;
using MonoSync.Attributes;

namespace MonoSync.Utils
{
    public class SyncPropertyInfo
    {
        public SyncPropertyInfo(SynchronizeAttribute synchronizeAttribute, PropertyInfo propertyInfo)
        {
            SynchronizeAttribute = synchronizeAttribute;
            PropertyInfo = propertyInfo;
        }

        public PropertyInfo PropertyInfo { get; }
        public SynchronizeAttribute SynchronizeAttribute { get; }
    }
}