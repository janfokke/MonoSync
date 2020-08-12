using System;

namespace MonoSync
{
    public class Settings
    {
        public static Func<Settings> Default { get; set; } = () => new Settings();

        public SerializerCollection Serializers { get; } = new SerializerCollection();
        public SynchronizerCollection Synchronizers { get; } = new SynchronizerCollection();
    }
}