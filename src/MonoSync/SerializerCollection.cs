using System;
using System.Collections.Generic;
using MonoSync.Exceptions;
using MonoSync.Serializers;

namespace MonoSync
{
    public class SerializerCollection
    {
        protected readonly Dictionary<Type, ISerializer> CachedSerializers = new Dictionary<Type, ISerializer>();
        protected readonly IList<ISerializer> Serializers = new List<ISerializer>();

        public SerializerCollection()
        {
            AddSerializer(new BooleanSerializer());
            AddSerializer(new CharSerializer());

            AddSerializer(new SingleSerializer());
            AddSerializer(new DoubleSerializer());
            AddSerializer(new DecimalSerializer());

            AddSerializer(new ByteSerializer());
            AddSerializer(new SByteSerializer());

            AddSerializer(new Int16Serializer());
            AddSerializer(new UInt16Serializer());

            AddSerializer(new Int32Serializer());
            AddSerializer(new UInt32Serializer());

            AddSerializer(new Int64Serializer());
            AddSerializer(new UInt64Serializer());

            AddSerializer(new GuidSerializer());
        }

        public ISerializer FindSerializerByType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            // Enums are serialized with their underlying type.
            if (type.IsEnum)
            {
                type = type.GetEnumUnderlyingType();
            }

            if (CachedSerializers.TryGetValue(type, out ISerializer serializer) == false)
            {
                // serializers are looped in reverse because the last added serializers should be prioritized.
                for (var i = Serializers.Count - 1; i >= 0; i--)
                {
                    ISerializer matchingSerializer = Serializers[i];
                    if (matchingSerializer.CanSerialize(type))
                    {
                        CachedSerializers.Add(type, matchingSerializer);
                        return matchingSerializer;
                    }
                }

                throw new SerializerNotFoundException(type);
            }
            return serializer;
        }

        public void AddSerializer(ISerializer serializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            Serializers.Add(serializer);
        }
    }
}