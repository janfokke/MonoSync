using System;
using MonoSync.Utils;

namespace MonoSync.Synchronizers
{
    public class StringTargetSynchronizer : TargetSynchronizer
    {
        public StringTargetSynchronizer(int referenceId) : base(referenceId)
        {
            
        }

        public override void Dispose()
        {
            // Ignore
        }

        public sealed override void Read(ExtendedBinaryReader reader)
        {
            if(Reference != null)
                throw new InvalidOperationException("Already initialized");
            Reference = reader.ReadString();
        }
    }
}