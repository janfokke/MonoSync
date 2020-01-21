using System;
using MonoSync.Utils;

namespace MonoSync
{
    public interface ITypeEncoder
    {
        Type ReadType(ExtendedBinaryReader reader);

        /// <summary>
        ///     Converts type into a compact integer array which can be decoded using <see cref="TypeEncoder.ReadType" />
        /// </summary>
        /// <returns></returns>
        void WriteType(Type type, ExtendedBinaryWriter writer);
    }
}