namespace MonoSync
{
    public class SyncSourceProperty
    {
        public readonly IFieldSerializer FieldSerializer;
        public readonly bool IsValueType;
        public short Index;
        public string Name { get; }

        public SyncSourceProperty(short index, string name, IFieldSerializer fieldSerializer, bool isValueType)
        {
            Name = name;
            FieldSerializer = fieldSerializer;
            IsValueType = isValueType;
            Index = index;
        }
    }
}