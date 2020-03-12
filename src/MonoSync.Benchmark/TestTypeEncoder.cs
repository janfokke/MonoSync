namespace MonoSync.Benchmark
{
    class TestTypeEncoder : TypeEncoder
    {
        public TestTypeEncoder()
        {
            var index = ReservedIdentifiers.StartingIndexNonReservedTypes;
            RegisterType<World>(index++);
            RegisterType<Entity>(index++);
        }
    }
}