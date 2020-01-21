using System;
using MonoSync.Utils;

namespace MonoSync.SyncSource
{
    public interface IFieldSerializer
    {
        bool CanInterpolate => false;
        bool CanSerialize(Type type);
        void Serialize(object value, ExtendedBinaryWriter writer);

        object Interpolate(object source, object target, float factor)
        {
            throw new NotImplementedException();
        }

        void Deserialize(ExtendedBinaryReader reader, Action<object> valueFixup);
    }

    public interface IFieldSerializer<T> : IFieldSerializer
    {
        // Default type check
        bool IFieldSerializer.CanSerialize(Type type)
        {
            return type == typeof(T);
        }

        object IFieldSerializer.Interpolate(object source, object target, float factor)
        {
            return Interpolate((T) source, (T) target, factor);
        }

        T Interpolate(T source, T target, float factor)
        {
            throw new NotImplementedException();
        }


        void IFieldSerializer.Serialize(object value, ExtendedBinaryWriter writer)
        {
            Serialize((T) value, writer);
        }

        void Serialize(T value, ExtendedBinaryWriter writer);

        void IFieldSerializer.Deserialize(ExtendedBinaryReader reader, Action<object> valueFixup)
        {
            Deserialize(reader, t => valueFixup(t));
        }

        /// <param name="valueFixup">
        ///     Because reference types may not be read yet, the deserialization is fixed up when it becomes
        ///     available
        /// </param>
        void Deserialize(ExtendedBinaryReader reader, Action<T> valueFixup);
    }
}