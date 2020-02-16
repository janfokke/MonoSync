using System;

namespace MonoSync.PropertyStates
{
    public interface ISyncTargetPropertyState : IDisposable
    {
        void HandleRead(object reader);
    }
}