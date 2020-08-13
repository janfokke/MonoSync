using System;
using System.Collections.Generic;
using System.Text;
using MonoSync.Utils;

namespace MonoSync.Synchronizers
{
    class ArraySourceSynchronizer : SourceSynchronizer
    {
        private readonly ISerializer _elementSerializer;
        private new Array Reference => (Array) base.Reference;

        public ArraySourceSynchronizer(SourceSynchronizerRoot sourceSynchronizerRoot, int referenceId, object reference) : base(sourceSynchronizerRoot, referenceId, reference)
        {
            Type type = reference.GetType();
            _elementSerializer = sourceSynchronizerRoot.Settings.Serializers.FindSerializerByType(type.GetElementType());
            if (type.GetElementType().IsValueType == false)
            {
                foreach (object item in Reference)
                {
                    sourceSynchronizerRoot.Synchronize(item);
                }
            }
        }

        public override void WriteChanges(ExtendedBinaryWriter binaryWriter)
        {
            throw new NotImplementedException();
        }

        public override void WriteFull(ExtendedBinaryWriter binaryWriter)
        {
            var arrayRanks = Reference.GetType().GetArrayRank();
            var lengths = new int[arrayRanks];
            binaryWriter.Write7BitEncodedInt(arrayRanks);
            for (int rank = 0; rank < arrayRanks; rank++)
            {
                int length = Reference.GetLength(rank);
                lengths[rank] = length;
                binaryWriter.Write7BitEncodedInt(length);
            }
            int[] zIndex = new int[arrayRanks];
            void TraverseArray(int depth)
            {
                int length = lengths[depth];
                if (depth + 1 < arrayRanks)
                {
                    for (int i = 0; i < length; i++)
                    {
                        zIndex[depth] = i;
                        TraverseArray(depth + 1);
                    }
                }
                else
                {
                    for (int i = 0; i < length; i++)
                    {
                        zIndex[depth] = i;
                        _elementSerializer.Write(Reference.GetValue(zIndex), binaryWriter);
                    }
                }
            }
            TraverseArray(0);
        }
    }
}
