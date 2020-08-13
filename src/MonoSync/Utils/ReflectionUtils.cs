using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MonoSync.Exceptions;

namespace MonoSync.Utils
{
    internal static class ReflectionUtils
    {
        public static Func<object, object> CompilePropertyGetter(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentNullException(nameof(propertyInfo));
            }

            if (propertyInfo.CanRead == false)
            {
                return null;
            }

            var param = Expression.Parameter(typeof(object));
            var instance = Expression.Convert(param, propertyInfo.DeclaringType);
            var convert = Expression.TypeAs(Expression.Property(instance, propertyInfo), typeof(object));
            return Expression.Lambda<Func<object, object>>(convert, param).Compile();
        }

        public static Action<object, object> CompilePropertySetter(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentNullException(nameof(propertyInfo));
            }

            if (propertyInfo.CanWrite == false)
            {
                return null;
            }

            var param = Expression.Parameter(typeof(object));
            var argument = Expression.Parameter(typeof(object));
            var expression = Expression.Convert(param, propertyInfo.DeclaringType);
            var methodInfo = propertyInfo.SetMethod;
            var arguments = Expression.Convert(argument, propertyInfo.PropertyType);
            var setterCall = Expression.Call(expression, methodInfo, arguments);
            return Expression.Lambda<Action<object, object>>(setterCall, param, argument).Compile();
        }

        public static Func<object, object> CompileFieldGetter(FieldInfo fieldInfo)
        {
            var param = Expression.Parameter(typeof(object));
            var instance = Expression.Convert(param, fieldInfo.DeclaringType);
            var convert = Expression.TypeAs(Expression.Field(instance, fieldInfo), typeof(object));
            return Expression.Lambda<Func<object, object>>(convert, param).Compile();
        }

        public static Action<object, object> CompileFieldSetter(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
            {
                throw new ArgumentNullException(nameof(fieldInfo));
            }

            if (fieldInfo.IsInitOnly)
            {
                return null;
            }

            ParameterExpression referenceParameter = Expression.Parameter(typeof(object));
            ParameterExpression valueParameter = Expression.Parameter(typeof(object));

            UnaryExpression reference = Expression.Convert(referenceParameter, fieldInfo.DeclaringType);
            UnaryExpression value = Expression.Convert(valueParameter, fieldInfo.FieldType);

            MemberExpression fieldExp = Expression.Field(reference, fieldInfo);
            BinaryExpression assignExp = Expression.Assign(fieldExp, value);
            return Expression.Lambda<Action<object, object>>(assignExp, referenceParameter, valueParameter).Compile();
        }
    }
}
