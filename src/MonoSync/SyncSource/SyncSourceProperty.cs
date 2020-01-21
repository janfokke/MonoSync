using System;
using MonoSync.Utils;

namespace MonoSync.SyncSource
{
    public class SyncSourceProperty : SyncProperty
    {
        private readonly IFieldSerializer _fieldSerializer;
        private readonly Func<object> _getter;

        public SyncSourceProperty(int index, Type propertyType, Func<object> getter, IFieldSerializer fieldSerializer) :
            base(index)
        {
            PropertyType = propertyType;
            _getter = getter;
            _fieldSerializer = fieldSerializer;
        }

        public Type PropertyType { get; }

        internal object Value { get; private set; }

        internal object PreviousValue { get; private set; }

        internal void UpdateValue()
        {
            PreviousValue = Value;
            Value = _getter();
        }

        internal void Serialize(ExtendedBinaryWriter writer)
        {
            _fieldSerializer.Serialize(Value, writer);
        }
    }
}