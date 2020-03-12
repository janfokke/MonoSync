using System;
using MonoSync.Exceptions;

namespace MonoSync
{
    public interface IFieldSerializerResolver
    {
        /// <summary>Resolves the serializer for a field.</summary>
        /// <param name="type">The field type.</param>
        /// <exception cref="FieldSerializerNotFoundException"></exception>
        IFieldSerializer ResolveSerializer(Type type);

        /// <summary>
        ///     Adds the serializer to resolvable serializers.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        void AddSerializer(IFieldSerializer serializer);
    }
}