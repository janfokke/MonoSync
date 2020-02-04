using System;
using MonoSync.SyncSource;
using MonoSync.Utils;

namespace MonoSync
{
    public abstract class FieldSerializer<T> : IFieldSerializer
    {
        public virtual bool CanInterpolate => false;

        public virtual bool CanSerialize(Type type)
        {
            return type == typeof(T);
        }

        public virtual object Interpolate(object source, object target, float factor)
        {
            return Interpolate((T) source, (T) target, factor);
        }

        public virtual void Serialize(object value, ExtendedBinaryWriter writer)
        {
            Serialize((T) value, writer);
        }

        public virtual void Deserialize(ExtendedBinaryReader reader, Action<object> valueFixup)
        {
            Deserialize(reader, (Action<T>) (value => valueFixup(value)));
        }

        public virtual T Interpolate(T source, T target, float factor)
        {
            throw new NotImplementedException();
        }

        public abstract void Serialize(T value, ExtendedBinaryWriter writer);

        /// <param name="valueFixup">
        ///     Because reference types may not be read yet, the deserialization is fixed up when it becomes
        ///     available
        /// </param>
        public abstract void Deserialize(ExtendedBinaryReader reader, Action<T> valueFixup);
    }
}