using System;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class TargetReferenceFieldSerializer : IFieldSerializer
    {
        private readonly IReferenceResolver _referenceResolver;

        public TargetReferenceFieldSerializer(IReferenceResolver referenceResolver)
        {
            _referenceResolver = referenceResolver;
        }

        public bool CanSerialize(Type type)
        {
            return type.IsValueType == false;
        }

        public void Write(object value, ExtendedBinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public object Interpolate(object source, object target, float factor)
        {
            throw new NotImplementedException();
        }

        public void Read(ExtendedBinaryReader reader, Action<object> valueFixup)
        {
            var referenceId = reader.Read7BitEncodedInt();
            _referenceResolver.ResolveReference(referenceId, valueFixup);
        }
    }
}