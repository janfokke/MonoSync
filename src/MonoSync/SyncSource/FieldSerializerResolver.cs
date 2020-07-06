using System;
using System.Collections.Generic;
using MonoSync.Exceptions;
using MonoSync.FieldSerializers;

namespace MonoSync
{
    public class FieldSerializerResolver : IFieldSerializerResolver
    {
        protected readonly Dictionary<Type, IFieldSerializer> CachedSerializers = new Dictionary<Type, IFieldSerializer>();
        protected readonly IList<IFieldSerializer> Serializers = new List<IFieldSerializer>();
        protected readonly SourceReferenceFieldSerializer SourceReferenceFieldSerializer;
        protected readonly TargetReferenceFieldSerializer TargetReferenceFieldSerializer;

        public FieldSerializerResolver(IReferenceResolver referenceResolver) : this()
        {
            if (referenceResolver == null)
            {
                throw new ArgumentNullException(nameof(referenceResolver));
            }

            TargetReferenceFieldSerializer = new TargetReferenceFieldSerializer(referenceResolver);
        }

        public FieldSerializerResolver(IIdentifierResolver identifierResolver) : this()
        {
            if (identifierResolver == null)
            {
                throw new ArgumentNullException(nameof(identifierResolver));
            }

            SourceReferenceFieldSerializer = new SourceReferenceFieldSerializer(identifierResolver);
        }

        private FieldSerializerResolver()
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
        }

        public IFieldSerializer ResolveSerializer(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            // Reference serializers
            if (type.IsValueType == false)
            {
                if (TargetReferenceFieldSerializer != null)
                {
                    return TargetReferenceFieldSerializer;
                }

                return SourceReferenceFieldSerializer;
            }

            // Enums are serialized with their underlying type.
            if (type.IsEnum)
            {
                type = type.GetEnumUnderlyingType();
            }

            if (CachedSerializers.TryGetValue(type, out IFieldSerializer serializer) == false)
            {
                // serializers are looped in reverse because the last added serializers should be prioritized.
                for (var i = Serializers.Count - 1; i >= 0; i--)
                {
                    IFieldSerializer matchingSerializer = Serializers[i];
                    if (matchingSerializer.CanSerialize(type))
                    {
                        CachedSerializers.Add(type, matchingSerializer);
                        return matchingSerializer;
                    }
                }

                throw new FieldSerializerNotFoundException(type);
            }

            return serializer;
        }

        public void AddSerializer(IFieldSerializer serializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            Serializers.Add(serializer);
        }
    }
}