using System;
using MonoSync.Utils;

namespace MonoSync
{
    public abstract class FieldSerializer<T> : IFieldSerializer
    {
        public virtual bool CanSerialize(Type type)
        {
            return type == typeof(T);
        }

        public virtual object Interpolate(object source, object target, float factor)
        {
            return Interpolate((T) source, (T) target, factor);
        }

        public virtual void Write(object value, ExtendedBinaryWriter writer)
        {
            Write((T) value, writer);
        }

        public virtual void Read(ExtendedBinaryReader reader, Action<object> synchronizationCallback)
        {
            Read(reader, (Action<T>) (value => synchronizationCallback(value)));
        }

        public virtual T Interpolate(T source, T target, float factor)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="Write(object,MonoSync.Utils.ExtendedBinaryWriter)" />
        public abstract void Write(T value, ExtendedBinaryWriter writer);

        /// <inheritdoc cref="Read(MonoSync.Utils.ExtendedBinaryReader,System.Action{object})" />
        public abstract void Read(ExtendedBinaryReader reader, Action<T> synchronizationCallback);
    }
}