﻿using System;
using MonoSync.Utils;

namespace MonoSync.Serializers
{
    public class Int32Serializer : Serializer<int>
    {
        public override void Write(int value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public override void Read(ExtendedBinaryReader reader, Action<int> synchronizationCallback)
        {
            synchronizationCallback(reader.ReadInt32());
        }

        public override int Interpolate(int source, int target, float factor)
        {
            return (int) (source + (target - source) * factor);
        }
    }
}