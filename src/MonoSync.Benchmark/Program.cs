using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using Newtonsoft.Json;

namespace MonoSync.Benchmark
{
    class Program
    {
        static void Main()
        {
            RunMonoSyncTest(1000);
            RunMonoSyncTest(1000000);

            //RunJsonTest(1000);
            //RunJsonTest(1000000);

            Console.ReadLine();
        }

        private static void RunJsonTest(int entityCount)
        {
            // Initialization
            Console.Write("Running RunJsonTest test");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var world = new JsonWorld();
            for (int i = 0; i < entityCount; i++)
            {
                world.Entities.Add(i, new Entity());
            }
            JsonConvert.SerializeObject(world);
            stopwatch.Stop();
            Console.WriteLine("FullWrite:" + stopwatch.ElapsedMilliseconds);
        }

        private static void RunMonoSyncTest(int entityCount)
        {
            Console.WriteLine();
            Console.WriteLine($"Benchmark with {entityCount} entities");

            // Initialization
            Console.Write("Initializing: ");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var world = new MonoSyncWorld();
            var syncSourceRoot = new SourceSynchronizerRoot(world);

            using (world.Entities.BeginMassUpdate())
            {
                for (int i = 0; i < entityCount; i++)
                {
                    world.Entities.Add(i, new Entity());
                }
            }
            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds + "MS");
            stopwatch.Reset();
            
            // Full write
            Console.Write("Full write: ");
            stopwatch.Start();
            using (WriteSession writeSession = syncSourceRoot.BeginWrite())
            {
                writeSession.WriteFull();
            }
            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds + "MS");
            stopwatch.Reset();

            int changes = entityCount / 10;
            Console.Write($"{changes} changes write: ");
            stopwatch.Start();
            
            for (int i = 0; i < changes; i++)
            {
                world.Entities[i].XPos = 2;
            }
            using (WriteSession writeSession = syncSourceRoot.BeginWrite())
            {
                int size = writeSession.WriteChanges().SetTick(TimeSpan.Zero).Length;
            }
            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds + "MS");
        }
    }
}
