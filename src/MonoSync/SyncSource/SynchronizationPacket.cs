using System.IO;
using MonoSync.Utils;

namespace MonoSync.SyncSource
{
    /// <summary>
    ///     Helper class to pad Synchronization packets with their synchronization tick.
    /// </summary>
    public class SynchronizationPacket
    {
        private readonly byte[] _synchronizationPacket;

        public SynchronizationPacket(byte[] synchronizationPacket)
        {
            _synchronizationPacket = synchronizationPacket;
        }

        /// <summary>
        ///     Foreach connection, pad with connection synchronizationTick.
        /// </summary>
        /// <param name="synchronizationTick"></param>
        /// <returns></returns>
        public byte[] SetTick(int synchronizationTick)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new ExtendedBinaryWriter(memoryStream);
            writer.Write7BitEncodedInt(synchronizationTick);
            writer.Write(_synchronizationPacket);
            return memoryStream.ToArray();
        }
    }
}