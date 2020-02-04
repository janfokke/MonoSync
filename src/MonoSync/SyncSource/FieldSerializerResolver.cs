using System;
using System.Collections.Generic;
using MonoSync.Exceptions;
using MonoSync.FieldSerializers;

namespace MonoSync.SyncSource
{
    public class FieldSerializerResolver : IFieldSerializerResolver
    {
        private readonly IList<IFieldSerializer> _serializers = new List<IFieldSerializer>();

        public FieldSerializerResolver(IReferenceResolver referenceResolver)
        {
            AddSerializer(new BooleanFieldSerializer());
            AddSerializer(new CharFieldSerializer());

            AddSerializer(new SingleFieldSerializer());
            AddSerializer(new DoubleFieldSerializer());
            AddSerializer(new DecimalFieldSerializer());

            AddSerializer(new ByteFieldSerializer());
            AddSerializer(new SByteFieldSerializer());

            AddSerializer(new Int16FieldSerializer());
            AddSerializer(new UInt16FieldSerializer());

            AddSerializer(new Int32FieldSerializer());
            AddSerializer(new UInt32FieldSerializer());

            AddSerializer(new Int64FieldSerializer());
            AddSerializer(new UInt64FieldSerializer());

            AddSerializer(new GuidSerializer());

            AddSerializer(new ReferenceFieldSerializer(referenceResolver));
        }

        public IFieldSerializer FindMatchingSerializer(Type type)
        {
            if (type.IsEnum) type = type.GetEnumUnderlyingType();

            // serializers are looped in reverse because the last added serializers should be prioritized.
            for (var i = _serializers.Count - 1; i >= 0; i--)
                if (_serializers[i].CanSerialize(type))
                    return _serializers[i];

            throw new FieldSerializerNotFoundException(type);
        }

        public void AddSerializer(IFieldSerializer serializer)
        {
            _serializers.Add(serializer);
        }
    }
}