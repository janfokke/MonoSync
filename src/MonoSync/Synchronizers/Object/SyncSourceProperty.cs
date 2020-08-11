using System;
using MonoSync.Utils;

namespace MonoSync.Synchronizers
{
    public class SyncSourceProperty
    {
        private readonly ISerializer _serializer;
        public readonly bool IsValueType;
        private readonly Func<object> _getter;
        public short Index;
        public string Name { get; }

        public object Value => _getter();

        public SyncSourceProperty(short index, string name, ISerializer serializer, bool isValueType, Func<object> getter)
        {
            Name = name;
            _serializer = serializer;
            IsValueType = isValueType;
            _getter = getter;
            Index = index;
        }

        public void Serialize(ExtendedBinaryWriter writer)
        {
            _serializer.Write(Value, writer);
        }
    }
}