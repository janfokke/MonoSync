using System;
using MonoSync.Utils;

namespace MonoSync.Serializers
{
    public class SourceReferenceSerializer : ISerializer
    {
        private readonly IIdentifierResolver _identifierResolver;

        public SourceReferenceSerializer(IIdentifierResolver identifierResolver)
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