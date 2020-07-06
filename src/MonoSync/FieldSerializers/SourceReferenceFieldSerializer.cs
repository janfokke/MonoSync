using System;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class SourceReferenceFieldSerializer : IFieldSerializer
    {
        private readonly IIdentifierResolver _identifierResolver;

        public SourceReferenceFieldSerializer(IIdentifierResolver identifierResolver)
        {
            _identifierResolver = identifierResolver;
        }

        public bool CanSerialize(Type type)
        {
            return type.IsValueType == false;
        }

        public void Write(object value, ExtendedBinaryWriter writer)
        {
            var referenceIdentifier = _identifierResolver.ResolveIdentifier(value);
            writer.Write7BitEncodedInt(referenceIdentifier);
        }

        public object Interpolate(object source, object target, float factor)
        {
            throw new NotImplementedException();
        }

        public void Read(ExtendedBinaryReader reader, Action<object> synchronizationCallback)
        {
            throw new NotImplementedException();
        }
    }
}