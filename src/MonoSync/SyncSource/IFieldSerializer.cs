using System;
using MonoSync.Utils;

namespace MonoSync.SyncSource
{
    public interface IFieldSerializer
    {
        bool CanInterpolate { get; }
        bool CanSerialize(Type type);
        void Serialize(object value, ExtendedBinaryWriter writer);

        object Interpolate(object source, object target, float factor);

        void Deserialize(ExtendedBinaryReader reader, Action<object> valueFixup);
    }
}