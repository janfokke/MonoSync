using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MonoSync.Exceptions;
using MonoSync.Synchronizers;

namespace MonoSync
{
    public static class SyncExtensions
    {
        /// <summary>
        ///     Returns a single <see cref="SyncTargetProperty" />, and throws an exception if there is not exactly one
        ///     <see cref="SyncTargetProperty" />
        /// </summary>
        /// <param name="source">The Synchronization-object that contains the target member</param>
        /// <param name="selector">The Expression to the intended member Property</param>
        public static SyncTargetProperty GetSyncTargetProperty<TSyncType>(this TSyncType source,
            Expression<Func<TSyncType, object>> selector)
        {
            return GetSyncTargetProperties(source, selector).Single();
        }

        /// <summary>
        ///     Returns all the <see cref="SyncTargetProperty">SyncTargetProperties</see> that are bound to the Property
        /// </summary>
        /// <param name="source">The Synchronization-object that contains the target member</param>
        /// <param name="selector">The Expression to the intended member Property</param>
        public static IEnumerable<SyncTargetProperty> GetSyncTargetProperties<TSyncType>(this TSyncType source,
            Expression<Func<TSyncType, object>> selector)
        {
            var propertyName = GetMemberName(selector.Body);
            List<NotifyPropertyChangedTargetSynchronizer> syncTargetObjects = GetSyncTargetObjects(source).ToList();
            if (syncTargetObjects.Count == 0)
            {
                throw new SyncTargetPropertyNotFoundException(propertyName);
            }

            foreach (NotifyPropertyChangedTargetSynchronizer targetObject in syncTargetObjects)
            {
                yield return targetObject.GetSyncTargetProperty(propertyName);
            }
        }

        /// <summary>
        ///     The <see cref="NotifyPropertyChangedTargetSynchronizer" /> is resolved by scanning the
        ///     <see cref="INotifyPropertyChanged.PropertyChanged" /> event delegate targets
        /// </summary>
        private static IEnumerable<NotifyPropertyChangedTargetSynchronizer> GetSyncTargetObjects<T>(this T sync)
        {
            return GetSyncTargetObjects(sync.GetType(), sync);
        }

        public static IEnumerable<NotifyPropertyChangedTargetSynchronizer> GetSyncTargetObjects(Type type, object sync)
        {
            FieldInfo fieldInfo = null;
            while (type != null)
            {
                fieldInfo = type.GetField(nameof(INotifyPropertyChanged.PropertyChanged),
                    BindingFlags.Instance | BindingFlags.NonPublic);

                if (fieldInfo != null)
                {
                    break;
                }

                type = type.BaseType;
            }

            if (fieldInfo == null)
            {
                throw new Exception($"{nameof(INotifyPropertyChanged.PropertyChanged)} not found");
            }

            var eventDelegate =
                // ReSharper disable once PossibleNullReferenceException
                (MulticastDelegate) fieldInfo
                    .GetValue(sync);

            var syncSyncTargetObjects = new List<NotifyPropertyChangedTargetSynchronizer>();

            if (eventDelegate != null)
            {
                foreach (Delegate handler in eventDelegate.GetInvocationList())
                {
                    if (handler.Target is NotifyPropertyChangedTargetSynchronizer syncSyncTargetObject)
                    {
                        syncSyncTargetObjects.Add(syncSyncTargetObject);
                    }
                }
            }

            return syncSyncTargetObjects;
        }

        /// <summary>
        ///     Helper method to get the Property name from an expression
        /// </summary>
        private static string GetMemberName(Expression expression)
        {
            return expression switch
            {
                null => throw new ArgumentNullException(nameof(expression)),
                MemberExpression memberExpression => memberExpression.Member.Name,
                UnaryExpression unaryExpression when unaryExpression.Operand is MethodCallExpression methodExpression =>
                methodExpression.Method.Name,
                UnaryExpression unaryExpression => ((MemberExpression) unaryExpression.Operand).Member.Name,
                _ => throw new ArgumentException("Invalid expression.")
            };
        }
    }
}