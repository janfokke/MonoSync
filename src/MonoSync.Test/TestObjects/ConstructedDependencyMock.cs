using System;
using MonoSync.Attributes;
using MonoSync.SyncTargetObjects;
using PropertyChanged;

namespace MonoSync.Test.TestObjects
{
    [AddINotifyPropertyChangedInterface]
    public class ConstructedDependencyMock
    {
        public ISomeService SomeService { get; }

        public ConstructedDependencyMock()
        {
            
        }

        [SyncConstructor]
        protected ConstructedDependencyMock(ISomeService someService)
        {
            SomeService = someService;
        }
    }

    public class SomeServiceProvider : IServiceProvider
    {
        public SomeServiceProvider()
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

    public interface ISomeService
    {

    }

    public class SomeService : ISomeService
    {

    }
}