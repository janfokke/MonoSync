﻿using System;
using MonoSync.Utils;

namespace MonoSync.Serializers
{
    public class DoubleSerializer : Serializer<double>
    {
        public override void Write(double value, ExtendedBinaryWriter writer)
        {
            writer.Write(value);
        }

        public override void Read(ExtendedBinaryReader reader, Action<double> synchronizationCallback)
        {
            synchronizationCallback(reader.ReadDouble());
        }

        public override double Interpolate(double source, double target, float factor)
        {
            return source + (target - source) * factor;
        }
    }
}