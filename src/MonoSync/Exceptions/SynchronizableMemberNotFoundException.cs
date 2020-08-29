using System;
using MonoSync.Exceptions;

namespace MonoSync
{
    public class SynchronizableMemberNotFoundException : MonoSyncException
    {
        public SynchronizableMemberNotFoundException(Type declaringType,string memberName) : base($"SynchronizableMember {memberName} not found in {declaringType.Name}")
        {
        }
    }
}