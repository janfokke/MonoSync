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
        public static TProp SynchronizeProperty<TTarget, TProp>(this TTarget target, Expression<Func<TTarget, TProp>> selector, Func<TProp> defaultValue)
        {
            if (TryGetSyncTargetProperty(target, out var result, selector))
            {
                var syncTargetObjects = GetNotifyPropertyChangedTargetSynchronizer(target).ToList();
                if (syncTargetObjects.Any())
                {
                    var propertyName = GetMemberName(selector.Body);
                    var syncTargetProperty = syncTargetObjects.Single().GetSyncTargetProperty(propertyName);
                    return (TProp)syncTargetProperty.SynchronizedValue;
                }
            }
            return defaultValue();
        }

        /// <summary>
        ///     Returns a single <see cref="SyncPropertyAccessor" />, and throws an exception if there is not exactly one
        ///     <see cref="SyncPropertyAccessor" />
        /// </summary>
        /// <param name="source">The Synchronization-object that contains the target member</param>
        /// <param name="selector">The Expression to the intended member Property</param>
        public static SyncPropertyAccessor GetSyncTargetProperty<TSyncType>(this TSyncType source,
            Expression<Func<TSyncType, object>> selector)
        {
            return GetSyncTargetProperties(source, selector).Single();
        }

        public static bool TryGetSyncTargetProperty<TSyncType, TProp>(this TSyncType source, out SyncPropertyAccessor syncPropertyAccessor,
            Expression<Func<TSyncType, TProp>> selector)
        {
            var t = GetSyncTargetProperties(source, selector).FirstOrDefault();
            syncPropertyAccessor = t;
            return t != null;
        }

        /// <summary>
        ///     Returns all the <see cref="SyncPropertyAccessor">SyncTargetProperties</see> that are bound to the Property
        /// </summary>
        /// <param name="source">The Synchronization-object that contains the target member</param>
        /// <param name="selector">The Expression to the intended member Property</param>
        public static IEnumerable<SyncPropertyAccessor> GetSyncTargetProperties<TSyncType, TProp>(this TSyncType source,
            Expression<Func<TSyncType, TProp>> selector)
        {
            var propertyName = GetMemberName(selector.Body);
            List<ObjectTargetSynchronizer> syncTargetObjects = GetNotifyPropertyChangedTargetSynchronizer(source).ToList();
            foreach (ObjectTargetSynchronizer targetObject in syncTargetObjects)
            {
                yield return targetObject.GetSyncTargetProperty(propertyName);
            }
        }

        public static IEnumerable<ObjectTargetSynchronizer> GetNotifyPropertyChangedTargetSynchronizer(object reference)
        {
            foreach (WeakReference<TargetSynchronizerRoot> weakReference in TargetSynchronizerRoot.Instances)
            {
                if (weakReference.TryGetTarget(out TargetSynchronizerRoot targetSynchronizerRoot))
                {
                    var notifyPropertyChangedTargetSynchronizer = (ObjectTargetSynchronizer) targetSynchronizerRoot.ReferencePool.GetSyncObject(reference);
                    if(notifyPropertyChangedTargetSynchronizer != null)
                        yield return notifyPropertyChangedTargetSynchronizer;
                }
            }
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