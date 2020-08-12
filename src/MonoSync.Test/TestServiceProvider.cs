using System;

namespace MonoSync.Test.TestObjects
{
    public class TestServiceProvider : IServiceProvider
    {
        public TestServiceProvider()
        {
            SomeService = new SomeService();
        }

        public SomeService SomeService { get; }

        public object GetService(Type T)
        {
            if (T == typeof(ISomeService))
                return SomeService;
            throw new Exception($"Service of type {T} not found");
        }
    }
}