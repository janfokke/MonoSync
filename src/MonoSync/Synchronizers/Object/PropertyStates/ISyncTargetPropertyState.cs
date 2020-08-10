using System;

namespace MonoSync.Synchronizers.PropertyStates
{
    public interface ISyncTargetPropertyState : IDisposable
    {
        void HandleRead(object reader);
    }
}