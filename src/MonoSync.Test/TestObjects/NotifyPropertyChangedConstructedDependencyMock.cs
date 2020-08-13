using MonoSync.Attributes;
using PropertyChanged;

namespace MonoSync.Test.TestObjects
{
    [Synchronizable]
    [AddINotifyPropertyChangedInterface]
    public class NotifyPropertyChangedConstructedDependencyMock
    {
        public ISomeService SomeService { get; }

        public NotifyPropertyChangedConstructedDependencyMock()
        {
            
        }

        [SynchronizationConstructor]
        public NotifyPropertyChangedConstructedDependencyMock(ISomeService someService)
        {
            SomeService = someService;
        }
    }

    public interface ISomeService
    {

    }

    public class SomeService : ISomeService
    {

    }
}