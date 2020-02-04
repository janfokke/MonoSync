using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using MonoSync.Attributes;
using MonoSync.Exceptions;

namespace MonoSync.Utils
{
    internal class PropertyInfoHelper
    {
        public static IEnumerable<(PropertyInfo, SyncAttribute)> GetSyncProperties(PropertyInfo[] properties)
        {
            for (var i = 0; i < properties.Length; i++)
            {
                PropertyInfo propertyInfo = properties[i];
                SyncAttribute syncAttribute = GetSyncAttribute(propertyInfo);
                if (syncAttribute == null) continue;

                yield return (properties[i], syncAttribute);
            }
        }

        private static SyncAttribute GetSyncAttribute(PropertyInfo propertyInfo)
        {
            object[] attributes = propertyInfo.GetCustomAttributes(true);
            for (var index = 0; index < attributes.Length; index++)
            {
                object attribute = attributes[index];
                if (attribute is SyncAttribute syncAttribute) return syncAttribute;
            }

            return null;
        }


        public static Func<object> CreateGetterDelegate(PropertyInfo propertyInfo, object propertyOwner)
        {
            MethodInfo method = propertyInfo.GetGetMethod();
            if (method == null) throw new GetterNotFoundException(propertyInfo.Name);
            MethodCallExpression methodCall = Expression.Call(Expression.Constant(propertyOwner), method);
            UnaryExpression convert = Expression.Convert(methodCall, typeof(object));
            return Expression.Lambda<Func<object>>(convert).Compile();
        }

        public static Action<object> CreateSetterDelegate(PropertyInfo propertyInfo, object propertyOwner)
        {
            MethodInfo method = propertyInfo.GetSetMethod();
            if (method == null) return null;

            Type parameterType = method.GetParameters()[0].ParameterType;
            ParameterExpression parameter = Expression.Parameter(typeof(object), "value");
            MethodCallExpression methodCall = Expression.Call(Expression.Constant(propertyOwner), method,
                Expression.Convert(parameter, parameterType));
            return Expression.Lambda<Action<object>>(methodCall, parameter).Compile();
        }
    }
}