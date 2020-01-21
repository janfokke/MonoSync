using System;

namespace MonoSync.SyncTarget.PropertyStates
{
    public interface ISyncTargetPropertyState : IDisposable
    {
        void HandleRead(object reader);
    }
}