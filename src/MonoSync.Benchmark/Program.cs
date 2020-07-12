﻿using System;

namespace MonoSync.Benchmark
{
    class Program
    {
        static void Main()
        {
            RunTest(500);
            RunTest(1000000);
            Console.ReadLine();
        }

        private static void RunTest(int entityCount)
        {
            Console.WriteLine();
            Console.WriteLine($"Benchmark with {entityCount} entities");

            var world = new World();
            var syncSourceRoot = new SourceSynchronizerRoot(world);

            // Initialization
            Console.Write("Begin initializing: ");
            var date = DateTime.Now;
            using (world.Entities.BeginMassUpdate())
            {
                for (int i = 0; i < entityCount; i++)
                {
                    world.Entities.Add(i, new Entity());
                }
            }
            Console.WriteLine((DateTime.Now - date).TotalMilliseconds + " MS");

            // Full write
            Console.Write("Begin full write: ");
            date = DateTime.Now;
            using (WriteSession writeSession = syncSourceRoot.BeginWrite())
            {
                writeSession.WriteFull();
            }
            Console.WriteLine((DateTime.Now - date).TotalMilliseconds + " MS");

            // Changes
            Console.Write("Begin change only write: ");
            date = DateTime.Now;
            int changes = 100;
            for (int i = 0; i < changes; i++)
            {
                world.Entities[i].XPos = 2;
            }
            using (WriteSession writeSession = syncSourceRoot.BeginWrite())
            {
                int size = writeSession.WriteChanges().SetTick(0).Length;
                Console.WriteLine($"{(DateTime.Now - date).TotalMilliseconds * 1000} Micro sec, Changes: {changes}, Size in bytes: {size}");
            }
        }
    }
}
