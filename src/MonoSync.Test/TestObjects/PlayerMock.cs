using MonoSync.Attributes;

namespace MonoSync.Test.Synchronization
{
    [Synchronizable]
    public class PlayerMock
    {
        [Synchronize]
        public int Level { get; set; }
    }
}