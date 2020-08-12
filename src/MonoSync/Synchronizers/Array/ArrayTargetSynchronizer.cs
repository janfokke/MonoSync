using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoSync.Utils;

namespace MonoSync.Synchronizers
{
    class ArrayTargetSynchronizer : TargetSynchronizer
    {
        private readonly TargetSynchronizerRoot _synchronizerRoot;
        private readonly Type _type;
        private readonly ISerializer _elementSerializer;

        private new Array Reference
        {
            get => (Array) base.Reference;
            set => base.Reference = value;
        }

        public ArrayTargetSynchronizer(TargetSynchronizerRoot synchronizerRoot, int referenceId, Type type) : base(referenceId)
        {
            _synchronizerRoot = synchronizerRoot;
            _elementSerializer = synchronizerRoot.Settings.Serializers.FindSerializerByType(type.GetElementType());
            _type = type;
        }

        public override void Dispose()
        {
            
        }

        public override void Read(ExtendedBinaryReader binaryReader)
        {
            var arrayRanks = binaryReader.Read7BitEncodedInt();
            var lengths = new int[arrayRanks];
            for (int rank = 0; rank < arrayRanks; rank++)
            {
                int length = binaryReader.Read7BitEncodedInt();
                lengths[rank] = length;
            }

            Reference = Array.CreateInstance(_type.GetElementType(), lengths);
            
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
                        int[] zIndexClone = zIndex.ToArray();
                        _elementSerializer.Read(binaryReader, value => { Reference.SetValue(value, zIndexClone); });
                    }
                }
            }
            TraverseArray(0);
        }
    }
}
