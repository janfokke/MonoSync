using System.Collections;
using System.IO;

namespace MonoSync.Utils
{
    public class ExtendedBinaryReader : BinaryReader
    {
        public ExtendedBinaryReader(Stream stream) : base(stream)
        {
        }

        public new int Read7BitEncodedInt()
        {
            return base.Read7BitEncodedInt();
        }

        public BitArray ReadBitArray(int length)
        {
            byte[] bitArrayBytes = ReadBytes((length + 7) / 8);
            return new BitArray(bitArrayBytes);
        }
    }
}