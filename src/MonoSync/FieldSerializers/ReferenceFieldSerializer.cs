using System;
using MonoSync.SyncSource;
using MonoSync.Utils;

namespace MonoSync.FieldSerializers
{
    public class ReferenceFieldSerializer : IFieldSerializer<object>
    {
        private readonly IReferenceResolver _referenceResolver;

        public ReferenceFieldSerializer(IReferenceResolver referenceResolver)
        {
            _referenceResolver = referenceResolver;
        }

        public bool CanSerialize(Type type)
        {
            return type.IsValueType == false;
        }

        public void Serialize(object value, ExtendedBinaryWriter writer)
        {
            int referenceIdentifier = _referenceResolver.ResolveIdentifier(value);
            writer.Write7BitEncodedInt(referenceIdentifier);
        }

        public void Deserialize(ExtendedBinaryReader reader, Action<object> valueFixup)
        {
            int referenceId = reader.Read7BitEncodedInt();
            _referenceResolver.ResolveReference(referenceId, valueFixup);
        }
    }
}