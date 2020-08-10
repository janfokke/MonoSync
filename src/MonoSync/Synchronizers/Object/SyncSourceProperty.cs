namespace MonoSync.Synchronizers
{
    public class SyncSourceProperty
    {
        public readonly ISerializer Serializer;
        public readonly bool IsValueType;
        public short Index;
        public string Name { get; }

        public SyncSourceProperty(short index, string name, ISerializer serializer, bool isValueType)
        {
            Name = name;
            Serializer = serializer;
            IsValueType = isValueType;
            Index = index;
        }
    }
}