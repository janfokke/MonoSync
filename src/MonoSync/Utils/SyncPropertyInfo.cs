using System.Reflection;
using MonoSync.Attributes;

namespace MonoSync.Utils
{
    public class SyncPropertyInfo
    {
        public SyncPropertyInfo(SyncAttribute syncAttribute, PropertyInfo propertyInfo)
        {
            SyncAttribute = syncAttribute;
            PropertyInfo = propertyInfo;
        }

        public PropertyInfo PropertyInfo { get; }
        public SyncAttribute SyncAttribute { get; }
    }
}