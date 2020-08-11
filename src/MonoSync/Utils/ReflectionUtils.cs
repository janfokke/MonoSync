using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MonoSync.Exceptions;

namespace MonoSync.Utils
{
    static class ReflectionUtils
    {
        private static readonly ConcurrentDictionary<PropertyInfo, Func<object, object>> PropertyGetterCache = new ConcurrentDictionary<PropertyInfo, Func<object, object>>();
        public static bool TryResolvePropertyGetter(out Func<object, object> getter, PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentNullException(nameof(propertyInfo));
            }

            if (propertyInfo.CanRead == false)
            {
                getter = null;
                return false;
            }

            getter = PropertyGetterCache.GetOrAdd(propertyInfo, key =>
            {
                var param = Expression.Parameter(typeof(object));
                var instance = Expression.Convert(param, key.DeclaringType);
                var convert = Expression.TypeAs(Expression.Property(instance, key), typeof(object));
                return Expression.Lambda<Func<object, object>>(convert, param).Compile();
            });
            return true;
        }

        private static readonly ConcurrentDictionary<PropertyInfo, Action<object, object>> PropertySetterCache = new ConcurrentDictionary<PropertyInfo, Action<object, object>>();
        public static bool TryResolvePropertySetter(out Action<object, object> setter, PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentNullException(nameof(propertyInfo));
            }

            if (propertyInfo.CanWrite == false)
            {
                setter = null;
                return false;
            }

            setter = PropertySetterCache.GetOrAdd(propertyInfo, key =>
            {
                var param = Expression.Parameter(typeof(object));
                var argument = Expression.Parameter(typeof(object));
                var expression = Expression.Convert(param, key.DeclaringType);
                var methodInfo = key.SetMethod;
                var arguments = Expression.Convert(argument, key.PropertyType);
                var setterCall = Expression.Call(expression, methodInfo, arguments);
                return Expression.Lambda<Action<object, object>>(setterCall, param, argument).Compile();
            });
            return true;
        }
    }
}
