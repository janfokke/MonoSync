using System;
using System.IO;
using MonoSync.Utils;

namespace MonoSync
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
        public byte[] SetTick(TimeSpan synchronizationTick)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new ExtendedBinaryWriter(memoryStream);
            writer.Write(synchronizationTick.Ticks);
            writer.Write(_synchronizationPacket);
            return memoryStream.ToArray();
        }
    }
}