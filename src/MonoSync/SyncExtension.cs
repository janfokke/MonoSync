using System;
using System.Collections.Concurrent;
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
        public static TMember InitializeSynchronizableMember<TTarget, TMember>(this TTarget target, string name, Func<TMember> defaultValue)
        {
            if (TryGetSyncTargetMember(target, name, out SynchronizableTargetMember result))
            {
                return (TMember) result.SynchronizedValue;
            }
            return defaultValue();
        }

        /// <summary>
        ///     Returns a single <see cref="SynchronizableTargetMember" />, and throws an exception if there is not exactly one
        ///     <see cref="SynchronizableTargetMember" />
        /// </summary>
        /// <param name="source">The Synchronization-object that contains the target member</param>
        /// <param name="selector">The Expression to the intended member Value</param>
        public static SynchronizableTargetMember GetSyncTargetMember<TSyncType>(this TSyncType source, string name)
        {
            return GetSyncTargetMembers(source, name).Single();
        }

        public static bool TryGetSyncTargetMember<TSyncType>(this TSyncType source, string name, out SynchronizableTargetMember synchronizableTargetMember)
        {
            SynchronizableTargetMember targetMember = GetSyncTargetMembers(source, name).FirstOrDefault();
            synchronizableTargetMember = targetMember;
            return targetMember != null;
        }

        /// <summary>
        ///     Returns all the <see cref="SynchronizableTargetMember">SyncTargetProperties</see> that are bound to the Value
        /// </summary>
        /// <param name="source">The Synchronization-object that contains the target member</param>
        /// <param name="selector">The Expression to the intended member Value</param>
        public static IEnumerable<SynchronizableTargetMember> GetSyncTargetMembers<TSyncType>(this TSyncType source, string name)
        {
            IEnumerable<ObjectTargetSynchronizer> syncTargetObjects = GetTargetSynchronizer(source);
            foreach (ObjectTargetSynchronizer targetObject in syncTargetObjects)
            {
                yield return targetObject.GetSyncTargetMember(name);
            }
        }

        public static IEnumerable<ObjectTargetSynchronizer> GetTargetSynchronizer(object reference)
        {
            foreach (WeakReference<TargetSynchronizerRoot> weakReference in TargetSynchronizerRoot.Instances)
            {
                if (weakReference.TryGetTarget(out TargetSynchronizerRoot targetSynchronizerRoot))
                {
                    var notifyPropertyChangedTargetSynchronizer = (ObjectTargetSynchronizer) targetSynchronizerRoot.ReferencePool.GetSynchronizer(reference);
                    if(notifyPropertyChangedTargetSynchronizer != null)
                        yield return notifyPropertyChangedTargetSynchronizer;
                }
            }
        }
    }
}