using System;
using MonoSync.Utils;

namespace MonoSync
{
    public interface IFieldSerializer
    {
        /// <summary>
        ///     Determines whether this instance can serialize the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        bool CanSerialize(Type type);

        /// <summary>
        ///     Reads the data from the <see cref="reader" /> and deserializes the value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="valueFixup">Because reference types may not be read yet, the read value is fixed when it becomes available</param>
        void Read(ExtendedBinaryReader reader, Action<object> valueFixup);

        /// <summary>
        ///     Serializes and writes the <see cref="value" /> to the <see cref="writer" />.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="writer">The writer.</param>
        void Write(object value, ExtendedBinaryWriter writer);

        /// <summary>
        ///     Interpolates the <see cref="source" /> to the <see cref="target" /> with <see cref="factor" />.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="factor">The factor.</param>
        /// <returns>The interpolated value</returns>
        object Interpolate(object source, object target, float factor);
    }
}