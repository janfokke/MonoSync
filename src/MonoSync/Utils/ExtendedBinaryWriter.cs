using System.Collections;
using System.IO;

namespace MonoSync.Utils
{
    public class ExtendedBinaryWriter : BinaryWriter
    {
        public ExtendedBinaryWriter(Stream stream) : base(stream)
        {
        }

        public new void Write7BitEncodedInt(int i)
        {
            base.Write7BitEncodedInt(i);
        }

        public void Write(BitArray bitArray)
        {
            var bitArrayBytes = new byte[(bitArray.Length + 7) / 8];
            bitArray.CopyTo(bitArrayBytes, 0);
            Write(bitArrayBytes);
        }
    }
}