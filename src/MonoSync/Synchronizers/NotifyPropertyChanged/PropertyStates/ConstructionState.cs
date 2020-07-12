using MonoSync.Attributes;

namespace MonoSync.Synchronizers.PropertyStates
{
    /// <summary>
    ///     Only sets value on construction.
    ///     If <see cref="SyncAttribute" /> is used on a get only property this state will also be implicitly used.
    /// </summary>
    internal class ConstructionState : ISyncTargetPropertyState
    {
        public static ConstructionState Instance { get; } = new ConstructionState();

        public void Dispose()
        {
        }

        public void HandleRead(object reader)
        {
        }
    }
}