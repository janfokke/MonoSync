using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MonoSync.Synchronizers;

namespace MonoSync
{
    public static class SyncExtensions
    {
        public static void InvokeWhenConstructed<TSyncObject>(this TSyncObject syncObject, Action<TSyncObject> callback)
        {
            if(TryGetTargetSynchronizer(syncObject, out ObjectTargetSynchronizer objectTargetSynchronizer))
            {
                objectTargetSynchronizer.InvokeWhenConstructed(reference => callback((TSyncObject)reference));
            }
            else
            {
                callback(syncObject);
            }
        }

        public static TMember InitializeSynchronizableMember<TTarget, TMember>(this TTarget target, string name, Func<TMember> defaultValue)
        {
            if (TryGetSyncTargetMember(target, name, out SynchronizableTargetMember result))
            {
                return (TMember) result.SynchronizedValue;
            }
            return defaultValue();
        }

        public static bool TryGetSyncTargetMember<TSyncType>(this TSyncType source, string name, out SynchronizableTargetMember synchronizableTargetMember)
        {
            if (TryGetTargetSynchronizer(source, out ObjectTargetSynchronizer objectTargetSynchronizer))
            {
                return objectTargetSynchronizer.TryGetMemberByName(name, out synchronizableTargetMember);
            }
            synchronizableTargetMember = null;
            return false;
        }

        /// <summary>
        ///     Returns all the <see cref="SynchronizableTargetMember">SyncTargetProperties</see> that are bound to the Value
        /// </summary>
        public static SynchronizableTargetMember GetSyncTargetMember<TSyncType>(this TSyncType source, string name)
        {
            if (TryGetSyncTargetMember(source, name, out SynchronizableTargetMember targetMember))
            {
                return targetMember;
            }
            throw new SynchronizableMemberNotFoundException(typeof(TSyncType), name);
        }

        public static ObjectTargetSynchronizer GetTargetSynchronizer(object reference)
        {
            if (TryGetTargetSynchronizer(reference, out ObjectTargetSynchronizer objectTargetSynchronizer))
            {
                return objectTargetSynchronizer;
            }
            throw new ObjectTargetSynchronizerNotFoundException();
        }

        public static bool TryGetTargetSynchronizer(object reference, out ObjectTargetSynchronizer objectTargetSynchronizer)
        {
            foreach (WeakReference<TargetSynchronizerRoot> weakReference in TargetSynchronizerRoot.Instances)
            {
                if (weakReference.TryGetTarget(out TargetSynchronizerRoot targetSynchronizerRoot))
                {
                    objectTargetSynchronizer = (ObjectTargetSynchronizer) targetSynchronizerRoot.ReferencePool.GetSynchronizer(reference);
                    if(objectTargetSynchronizer != null)
                        return true;
                }
            }
            objectTargetSynchronizer = null;
            return false;
        }
    }
}